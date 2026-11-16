namespace DocuMind.Domain.Entities;

/// <summary>
/// Tenant-aware audit log entry capturing sensitive platform events.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? UserId { get; set; }
    public Guid? OrganizationId { get; set; }
    public required string Action { get; set; }
    public required string ResourceType { get; set; }
    public Guid? ResourceId { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? IpAddress { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public ApplicationUser? User { get; set; }
    public Organization? Organization { get; set; }
}

