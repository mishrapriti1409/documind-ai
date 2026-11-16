using DocuMind.Domain.Common;
using DocuMind.Domain.Enums;

namespace DocuMind.Domain.Entities;

/// <summary>
/// Uploaded business document metadata and processing status.
/// </summary>
public class Document : BaseEntity, ISoftDelete
{
    public Guid OrganizationId { get; set; }
    public Guid UploadedByUserId { get; set; }
    public required string OriginalFileName { get; set; }
    public required string StoragePath { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Category { get; set; }
    public string? Tags { get; set; }
    public ProcessingStatus ProcessingStatus { get; set; } = ProcessingStatus.Uploaded;
    public string? ProcessingError { get; set; }
    public int TotalPages { get; set; }
    public int TotalChunks { get; set; }
    public DateTimeOffset? ProcessedAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }

    // Navigation
    public Organization? Organization { get; set; }
    public ApplicationUser? UploadedBy { get; set; }
    public ICollection<DocumentChunk> Chunks { get; set; } = new List<DocumentChunk>();
    public ICollection<ProcessingJob> ProcessingJobs { get; set; } = new List<ProcessingJob>();
    public ICollection<DocumentVersion> Versions { get; set; } = new List<DocumentVersion>();
    public ICollection<DocumentPermission> Permissions { get; set; } = new List<DocumentPermission>();
}

