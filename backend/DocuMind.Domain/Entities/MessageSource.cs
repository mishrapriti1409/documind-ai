namespace DocuMind.Domain.Entities;

/// <summary>
/// Verifiable grounding citation linking an AI answer to a specific page excerpt.
/// </summary>
public class MessageSource
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MessageId { get; set; }
    public Guid ChunkId { get; set; }
    public Guid DocumentId { get; set; }
    public int PageNumber { get; set; }
    public required string Excerpt { get; set; }
    public float RelevanceScore { get; set; }

    // Navigation
    public Message? Message { get; set; }
    public DocumentChunk? Chunk { get; set; }
    public Document? Document { get; set; }
}

