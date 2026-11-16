using DocuMind.Domain.Common;

namespace DocuMind.Domain.Entities;

/// <summary>
/// Multi-tenant tenant entity. All data belongs to an Organization.
/// </summary>
public class Organization : BaseEntity
{
    public required string Name { get; set; }
    public required string Slug { get; set; }
    public string? Domain { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    public ICollection<Document> Documents { get; set; } = new List<Document>();
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}

