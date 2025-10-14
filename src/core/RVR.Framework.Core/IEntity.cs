namespace RVR.Framework.Core;

/// <summary>
/// Represents a base interface for all entities in the domain.
/// Provides common properties that every entity must have.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for the entity.
    /// </summary>
    Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was created.
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last modified.
    /// </summary>
    DateTime? ModifiedAt { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user who last modified the entity.
    /// </summary>
    string? ModifiedBy { get; set; }
}
