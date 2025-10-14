namespace RVR.Framework.Data.MongoDB.Models;

/// <summary>
/// Configuration options for MongoDB.
/// </summary>
public sealed class MongoDbOptions
{
    /// <summary>
    /// The MongoDB connection string.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// The database name.
    /// </summary>
    public required string DatabaseName { get; set; }
}
