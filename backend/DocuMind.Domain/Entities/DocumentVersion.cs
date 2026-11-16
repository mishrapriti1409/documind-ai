using DocuMind.Domain.Common;

namespace DocuMind.Domain.Entities;

/// <summary>
/// Historical version record of an updated document.
/// </summary>
public class DocumentVersion : BaseEntity
{
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public required string StoragePath { get; set; }
    public Guid UploadedByUserId { get; set; }

    // Navigation
    public Document? Document { get; set; }
    public ApplicationUser? UploadedBy { get; set; }
}

