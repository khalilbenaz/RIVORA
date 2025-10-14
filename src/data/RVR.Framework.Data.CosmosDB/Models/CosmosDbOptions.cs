namespace RVR.Framework.Data.CosmosDB.Models;

/// <summary>
/// Configuration options for Azure Cosmos DB.
/// </summary>
public sealed class CosmosDbOptions
{
    /// <summary>
    /// The Cosmos DB connection string.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// The database name.
    /// </summary>
    public required string DatabaseName { get; set; }
}
