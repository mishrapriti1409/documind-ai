using DocuMind.Application.DTOs.AI;
using DocuMind.Application.DTOs.Chat;
using DocuMind.Application.Interfaces;
using DocuMind.Domain.Entities;
using DocuMind.Domain.Enums;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace DocuMind.Application.Services;

public class ChatService : IChatService
{
    private readonly IConversationRepository _conversationRepo;
    private readonly IAIServiceClient _aiClient;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IConversationRepository conversationRepo,
        IAIServiceClient aiClient,
        ILogger<ChatService> logger)
    {
        _conversationRepo = conversationRepo;
        _aiClient = aiClient;
        _logger = logger;
    }

    public async Task<ConversationDto> CreateConversationAsync(
        CreateConversationRequest request,
        Guid userId,
        Guid organizationId,
        CancellationToken ct = default)
    {
        var conversationId = Guid.NewGuid();
        var docIdsJson = request.DocumentIds != null && request.DocumentIds.Any()
            ? JsonSerializer.Serialize(request.DocumentIds)
            : null;

        var conversation = new Conversation
        {
            Id = conversationId,
            UserId = userId,
            OrganizationId = organizationId,
            Title = string.IsNullOrWhiteSpace(request.Title) ? "New Conversation" : request.Title.Trim(),
            Scope = request.Scope,
            DocumentIdsJson = docIdsJson,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _conversationRepo.AddAsync(conversation, ct);
        await _conversationRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Created conversation {ConvId} for user {UserId}", conversationId, userId);

        return new ConversationDto(
            conversation.Id,
            conversation.Title,
            conversation.Scope,
            conversation.CreatedAt,
            conversation.UpdatedAt,
            0,
            request.DocumentIds
        );
    }

    public async Task<IEnumerable<ConversationDto>> GetConversationsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var conversations = await _conversationRepo.GetUserConversationsAsync(userId, page, pageSize, ct);

        return conversations.Select(c =>
        {
            List<Guid>? docIds = null;
            if (!string.IsNullOrEmpty(c.DocumentIdsJson))
            {
                try { docIds = JsonSerializer.Deserialize<List<Guid>>(c.DocumentIdsJson); } catch { }
            }

            return new ConversationDto(
                c.Id,
                c.Title,
                c.Scope,
                c.CreatedAt,
                c.UpdatedAt,
                c.Messages?.Count ?? 0,
                docIds
            );
        });
    }

    public async Task<ConversationDto?> GetConversationAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken ct = default)
    {
        var conv = await _conversationRepo.GetByIdAsync(conversationId, ct);
        if (conv == null || conv.UserId != userId) return null;

        List<Guid>? docIds = null;
        if (!string.IsNullOrEmpty(conv.DocumentIdsJson))
        {
            try { docIds = JsonSerializer.Deserialize<List<Guid>>(conv.DocumentIdsJson); } catch { }
        }

        return new ConversationDto(
            conv.Id,
            conv.Title,
            conv.Scope,
            conv.CreatedAt,
            conv.UpdatedAt,
            conv.Messages?.Count ?? 0,
            docIds
        );
    }

    public async Task<ChatResponse> SendMessageAsync(
        Guid conversationId,
        SendMessageRequest request,
        Guid userId,
        Guid organizationId,
        CancellationToken ct = default)
    {
        var conversation = await _conversationRepo.GetWithMessagesAsync(conversationId, ct);
        if (conversation == null || conversation.UserId != userId)
        {
            throw new KeyNotFoundException("Conversation not found.");
        }

        // 1. Record User Message
        var userMessage = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = MessageRole.User,
            Content = request.Question,
            TokensUsed = 0,
            EstimatedCost = 0m,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await _conversationRepo.AddMessageAsync(userMessage, ct);

        // 2. Build conversation history for LLM (last 6 messages)
        var history = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .TakeLast(6)
            .Select(m => new Dictionary<string, string>
            {
                ["role"] = m.Role == MessageRole.User ? "user" : "assistant",
                ["content"] = m.Content
            })
            .ToList();

        // 3. Resolve target documents filter
        List<Guid>? targetDocIds = request.DocumentIds;
        if (targetDocIds == null && !string.IsNullOrEmpty(conversation.DocumentIdsJson))
        {
            try { targetDocIds = JsonSerializer.Deserialize<List<Guid>>(conversation.DocumentIdsJson); } catch { }
        }

        // 4. Query AI microservice RAG engine
        var aiQuery = new QueryRequestDto(
            Question: request.Question,
            OrganizationId: organizationId,
            DocumentIds: targetDocIds,
            UserId: userId,
            ConversationId: conversationId,
            ConversationHistory: history
        );

        var aiResponse = await _aiClient.QueryAsync(aiQuery, ct);

        // 5. Record Assistant Message
        var assistantMessageId = Guid.NewGuid();
        var assistantMessage = new Message
        {
            Id = assistantMessageId,
            ConversationId = conversationId,
            Role = MessageRole.Assistant,
            Content = aiResponse.Answer,
            TokensUsed = aiResponse.TokensUsed,
            EstimatedCost = aiResponse.CostEstimate,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        await _conversationRepo.AddMessageAsync(assistantMessage, ct);

        // 6. Record grounded MessageSources
        var sourceDtos = new List<SourceReferenceDto>();
        foreach (var src in aiResponse.Sources)
        {
            var msgSource = new MessageSource
            {
                Id = Guid.NewGuid(),
                MessageId = assistantMessageId,
                ChunkId = src.ChunkId,
                DocumentId = src.DocumentId,
                PageNumber = src.PageNumber,
                Excerpt = src.Excerpt,
                RelevanceScore = src.RelevanceScore
            };
            await _conversationRepo.AddMessageSourceAsync(msgSource, ct);

            sourceDtos.Add(new SourceReferenceDto(
                src.ChunkId,
                src.DocumentId,
                $"Document {src.DocumentId}",
                src.PageNumber,
                src.Excerpt,
                src.RelevanceScore
            ));
        }

        // 7. Update conversation timestamp
        conversation.UpdatedAt = DateTimeOffset.UtcNow;
        await _conversationRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Answered question in conversation {ConvId} with {SourceCount} citations",
            conversationId, sourceDtos.Count);

        return new ChatResponse(
            assistantMessageId,
            aiResponse.Answer,
            sourceDtos,
            aiResponse.InsufficientEvidence,
            aiResponse.TokensUsed,
            aiResponse.CostEstimate
        );
    }

    public async Task<IEnumerable<MessageDto>> GetMessagesAsync(
        Guid conversationId,
        Guid userId,
        CancellationToken ct = default)
    {
        var conversation = await _conversationRepo.GetWithMessagesAsync(conversationId, ct);
        if (conversation == null || conversation.UserId != userId)
        {
            throw new KeyNotFoundException("Conversation not found.");
        }

        return conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new MessageDto(
                m.Id,
                m.Role.ToString(),
                m.Content,
                m.TokensUsed,
                m.EstimatedCost,
                m.Feedback?.Score,
                m.CreatedAt,
                m.Sources.Select(s => new SourceReferenceDto(
                    s.ChunkId,
                    s.DocumentId,
                    s.Document?.OriginalFileName ?? "Document",
                    s.PageNumber,
                    s.Excerpt,
                    s.RelevanceScore
                )).ToList()
            ));
    }

    public async Task SubmitFeedbackAsync(
        Guid messageId,
        Guid userId,
        int score,
        string? comment,
        CancellationToken ct = default)
    {
        var feedback = new UserFeedback
        {
            Id = Guid.NewGuid(),
            MessageId = messageId,
            UserId = userId,
            Score = Math.Clamp(score, 1, 5),
            Comment = comment?.Trim(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _conversationRepo.AddFeedbackAsync(feedback, ct);
        await _conversationRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Recorded feedback {Score} for message {MsgId}", score, messageId);
    }
}

