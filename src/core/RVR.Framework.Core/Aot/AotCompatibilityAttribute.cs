namespace RVR.Framework.Core.Aot;

/// <summary>
/// Marker attribute indicating that the decorated class has been verified as compatible
/// with Native AOT (Ahead-of-Time) compilation and trimming.
/// This attribute does not affect runtime behavior; it serves as documentation
/// for developers and static analysis tools.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
public sealed class AotCompatibilityAttribute : Attribute
{
    /// <summary>
    /// Optional description of any AOT considerations or limitations.
    /// </summary>
    public string? Remarks { get; set; }
}
