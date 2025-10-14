namespace RVR.Framework.Core;

/// <summary>
/// Marker interface for aggregate roots.
/// An aggregate root is an entity that defines a consistency boundary.
/// All operations on the aggregate must be performed through the root.
/// </summary>
/// <remarks>
/// This is a marker interface used to identify aggregate roots in the domain model.
/// It inherits from IEntity to ensure all aggregate roots have the base entity properties.
/// </remarks>
public interface IAggregateRoot : IEntity
{
}
