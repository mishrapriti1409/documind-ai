using DocuMind.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace DocuMind.Application.DTOs.Chat;

public record CreateConversationRequest(
    [Required] string Title,
    ConversationScope Scope = ConversationScope.AllDocuments,
    List<Guid>? DocumentIds = null
);

public record ConversationDto(
    Guid Id,
    string Title,
    ConversationScope Scope,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    int MessageCount,
    List<Guid>? DocumentIds = null
);

public record SendMessageRequest(
    [Required, MinLength(1), MaxLength(2000)] string Question,
    List<Guid>? DocumentIds = null
);

public record SourceReferenceDto(
    Guid ChunkId,
    Guid DocumentId,
    string DocumentName,
    int PageNumber,
    string Excerpt,
    float RelevanceScore
);

public record MessageDto(
    Guid Id,
    string Role,
    string Content,
    int TokensUsed,
    decimal EstimatedCost,
    int? FeedbackScore,
    DateTimeOffset CreatedAt,
    List<SourceReferenceDto> Sources
);

public record ChatResponse(
    Guid MessageId,
    string Answer,
    List<SourceReferenceDto> Sources,
    bool InsufficientEvidence,
    int TokensUsed,
    decimal CostEstimate
);

public record FeedbackRequest(
    int Score,
    string? Comment
);

