using DocuMind.Domain.Common;

namespace DocuMind.Domain.Entities;

/// <summary>
/// Chunked segment of a document with token count, page number, and vector embedding.
/// </summary>
public class DocumentChunk : BaseEntity
{
    public Guid DocumentId { get; set; }
    public Guid OrganizationId { get; set; }
    public int ChunkIndex { get; set; }
    public required string Content { get; set; }
    public int PageNumber { get; set; }
    public int TokenCount { get; set; }
    /// <summary>
    /// Vector embedding float array (1536 dim for text-embedding-3-small).
    /// Mapped to pgvector vector(1536) in Infrastructure.
    /// </summary>
    public float[]? Embedding { get; set; }
    /// <summary>
    /// Structural JSON metadata (e.g. section titles, formatting hints).
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation
    public Document? Document { get; set; }
}

