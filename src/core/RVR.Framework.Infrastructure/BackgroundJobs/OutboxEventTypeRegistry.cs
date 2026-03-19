using System.Collections.Concurrent;

namespace RVR.Framework.Infrastructure.BackgroundJobs;

/// <summary>
/// Static registry that maps event type names to their CLR <see cref="Type"/>.
/// This replaces <c>Type.GetType()</c> calls, which are incompatible with Native AOT.
/// Register all outbox event types at application startup.
/// </summary>
public static class OutboxEventTypeRegistry
{
    private static readonly ConcurrentDictionary<string, Type> Registry = new();

    /// <summary>
    /// Registers a type using its <see cref="Type.AssemblyQualifiedName"/> as the key.
    /// </summary>
    public static void Register<T>() where T : class
    {
        var type = typeof(T);
        var key = type.AssemblyQualifiedName
                  ?? type.FullName
                  ?? type.Name;
        Registry[key] = type;
    }

    /// <summary>
    /// Registers a type with an explicit type name key.
    /// </summary>
    public static void Register(string typeName, Type type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName);
        ArgumentNullException.ThrowIfNull(type);
        Registry[typeName] = type;
    }

    /// <summary>
    /// Attempts to resolve a <see cref="Type"/> from a previously registered type name.
    /// </summary>
    public static Type? Resolve(string typeName)
    {
        return Registry.TryGetValue(typeName, out var type) ? type : null;
    }
}
