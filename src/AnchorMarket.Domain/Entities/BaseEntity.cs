namespace AnchorMarket.Domain.Entities;

/// <summary>Base class for all domain entities, providing a unique identifier and audit timestamps.</summary>
public abstract class BaseEntity
{
    /// <summary>Gets the unique identifier for this entity.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>Gets the UTC timestamp when this entity was created.</summary>
    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;

    /// <summary>Gets the UTC timestamp when this entity was last updated, or null if never updated.</summary>
    public DateTimeOffset? UpdatedAt { get; protected set; }
}
