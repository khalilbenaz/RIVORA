namespace RVR.Framework.Data.ReadReplica;

/// <summary>
/// Routes database connection requests to the appropriate connection string
/// based on whether the operation is read-only or requires write access.
/// </summary>
public interface IDbConnectionRouter
{
    /// <summary>
    /// Gets the connection string for the requested operation type.
    /// </summary>
    /// <param name="isReadOnly">
    /// When <c>true</c>, returns a read replica connection string.
    /// When <c>false</c>, returns the primary (write) connection string.
    /// </param>
    /// <returns>The appropriate connection string.</returns>
    string GetConnectionString(bool isReadOnly);
}
