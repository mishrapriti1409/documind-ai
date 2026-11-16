using DocuMind.Domain.Common;
using DocuMind.Domain.Enums;

namespace DocuMind.Domain.Entities;

/// <summary>
/// Chat conversation thread with scope and document filters.
/// </summary>
public class Conversation : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid OrganizationId { get; set; }
    public required string Title { get; set; }
    public ConversationScope Scope { get; set; } = ConversationScope.AllDocuments;
    /// <summary>
    /// JSON array of specific document UUIDs if scoped to MultiDocument or SingleDocument.
    /// </summary>
    public string? DocumentIdsJson { get; set; }

    // Navigation
    public ApplicationUser? User { get; set; }
    public Organization? Organization { get; set; }
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}

