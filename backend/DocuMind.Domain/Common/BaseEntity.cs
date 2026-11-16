namespace DocuMind.Domain.Common;

/// <summary>
/// Abstract base entity providing common identity and timestamp properties.
/// All domain entities inherit from this class.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>Globally unique identifier for this entity.</summary>
    public Guid Id { get; init; } = Guid.NewGuid();

    /// <summary>UTC timestamp when the entity was first persisted.</summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>UTC timestamp of the last update. Null until first update.</summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

