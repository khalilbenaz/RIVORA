namespace RVR.Framework.Data.ReadReplica;

/// <summary>
/// Configuration options for multi-database read replica routing.
/// </summary>
public class ReadReplicaOptions
{
    /// <summary>
    /// Gets or sets the connection string used for write operations (primary database).
    /// </summary>
    public string WriteConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the connection strings used for read-only operations (replica databases).
    /// When empty, the <see cref="WriteConnectionString"/> is used for all operations.
    /// </summary>
    public string[] ReadConnectionStrings { get; set; } = [];
}
