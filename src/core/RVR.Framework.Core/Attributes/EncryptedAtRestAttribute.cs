namespace RVR.Framework.Core.Attributes;

/// <summary>
/// Attribut pour marquer les propriétés devant être chiffrées en base de données
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class EncryptedAtRestAttribute : Attribute
{
}
