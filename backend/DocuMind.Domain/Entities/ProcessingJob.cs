using DocuMind.Domain.Common;

namespace DocuMind.Domain.Entities;

/// <summary>
/// Asynchronous ingestion and processing job queue entry.
/// </summary>
public class ProcessingJob : BaseEntity
{
    public Guid DocumentId { get; set; }
    public string Status { get; set; } = "Pending";
    public string? ErrorMessage { get; set; }
    public int AttemptCount { get; set; } = 0;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }

    // Navigation
    public Document? Document { get; set; }
}

