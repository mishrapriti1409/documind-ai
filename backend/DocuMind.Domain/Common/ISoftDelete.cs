namespace DocuMind.Domain.Common;

/// <summary>
/// Marker interface for entities that support soft deletion.
/// When <see cref="DeletedAt"/> is set, the entity is treated as deleted
/// and filtered from default queries via a global EF query filter.
/// </summary>
public interface ISoftDelete
{
    /// <summary>UTC timestamp when the entity was soft-deleted. Null means active.</summary>
    DateTimeOffset? DeletedAt { get; set; }
}

