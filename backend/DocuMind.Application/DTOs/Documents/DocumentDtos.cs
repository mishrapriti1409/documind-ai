namespace DocuMind.Application.DTOs.Documents;

public record DocumentDto(
    Guid Id,
    string OriginalFileName,
    string ContentType,
    long FileSizeBytes,
    string? Category,
    string? Tags,
    string ProcessingStatus,
    string? ProcessingError,
    int TotalPages,
    int TotalChunks,
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt,
    string UploadedBy
);

public record DocumentListResponse(
    IEnumerable<DocumentDto> Items,
    int Total,
    int Page,
    int PageSize
);

public record ProcessingStatusDto(
    Guid DocumentId,
    string Status,
    string? ErrorMessage,
    int AttemptCount,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt
);

