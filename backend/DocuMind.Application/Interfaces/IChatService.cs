using DocuMind.Application.DTOs.Chat;

namespace DocuMind.Application.Interfaces;

public interface IChatService
{
    Task<ConversationDto> CreateConversationAsync(CreateConversationRequest request, Guid userId, Guid organizationId, CancellationToken ct = default);
    Task<IEnumerable<ConversationDto>> GetConversationsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<ConversationDto?> GetConversationAsync(Guid conversationId, Guid userId, CancellationToken ct = default);
    Task<ChatResponse> SendMessageAsync(Guid conversationId, SendMessageRequest request, Guid userId, Guid organizationId, CancellationToken ct = default);
    Task<IEnumerable<MessageDto>> GetMessagesAsync(Guid conversationId, Guid userId, CancellationToken ct = default);
    Task SubmitFeedbackAsync(Guid messageId, Guid userId, int score, string? comment, CancellationToken ct = default);
}

