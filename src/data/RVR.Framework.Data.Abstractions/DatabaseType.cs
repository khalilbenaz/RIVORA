namespace RVR.Framework.Data.Abstractions;

/// <summary>
/// Represents the supported database types for the RIVORA Framework.
/// </summary>
public enum DatabaseType
{
    /// <summary>
    /// Microsoft SQL Server database provider.
    /// </summary>
    SqlServer = 0,

    /// <summary>
    /// PostgreSQL database provider.
    /// </summary>
    PostgreSQL = 1,

    /// <summary>
    /// MySQL database provider.
    /// </summary>
    MySQL = 2,

    /// <summary>
    /// SQLite database provider.
    /// </summary>
    SQLite = 3
}
