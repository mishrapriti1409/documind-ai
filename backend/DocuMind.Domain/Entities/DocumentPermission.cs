using DocuMind.Domain.Enums;

namespace DocuMind.Domain.Entities;

/// <summary>
/// User permission grant for a specific document.
/// </summary>
public class DocumentPermission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid DocumentId { get; set; }
    public Guid UserId { get; set; }
    public DocumentPermissionType Permission { get; set; }
    public DateTimeOffset GrantedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Document? Document { get; set; }
    public ApplicationUser? User { get; set; }
}

