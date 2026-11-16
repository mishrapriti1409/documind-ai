namespace DocuMind.Application.DTOs.Analytics;

public record DashboardStatsDto(
    int TotalDocuments,
    int ProcessedDocuments,
    int FailedDocuments,
    int PendingDocuments,
    int TotalConversations,
    long TotalMessages,
    int ActiveUsersLast30Days,
    double AverageProcessingTimeSeconds,
    decimal TotalAICostEstimate
);

public record DocumentUploadTrendDto(
    DateOnly Date,
    int Count
);

public record UserActivityDto(
    Guid UserId,
    string UserName,
    int DocumentsUploaded,
    int QuestionsAsked,
    DateTimeOffset LastActive
);

