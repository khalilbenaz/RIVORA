namespace RVR.Framework.MultiTenancy.Security;

/// <summary>
/// Helper to generate row-level security (RLS) SQL policies for multi-tenant isolation.
/// Provides defense-in-depth at the database level, complementing EF Core query filters.
/// </summary>
public static class RowLevelSecurityHelper
{
    /// <summary>
    /// Generates SQL Server RLS policy for a tenant-scoped table.
    /// Creates a security predicate function and applies filter/block predicates
    /// that restrict data access to the tenant set in SESSION_CONTEXT.
    /// </summary>
    /// <param name="schemaName">The database schema (e.g., "dbo").</param>
    /// <param name="tableName">The table to protect.</param>
    /// <param name="tenantColumnName">The column that holds the tenant identifier. Defaults to "TenantId".</param>
    /// <returns>SQL script that creates the RLS policy.</returns>
    public static string GenerateSqlServerPolicy(string schemaName, string tableName, string tenantColumnName = "TenantId")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(schemaName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantColumnName);

        return $"""
            -- Row-Level Security policy for {schemaName}.{tableName}
            -- Ensures tenants can only access their own data at the database level

            -- 1. Create security predicate function
            CREATE OR ALTER FUNCTION {schemaName}.fn_tenant_filter_{tableName}(@TenantId UNIQUEIDENTIFIER)
            RETURNS TABLE
            WITH SCHEMABINDING
            AS
            RETURN SELECT 1 AS result
            WHERE @TenantId = CAST(SESSION_CONTEXT(N'TenantId') AS UNIQUEIDENTIFIER);

            -- 2. Create security policy
            CREATE SECURITY POLICY {schemaName}.TenantPolicy_{tableName}
            ADD FILTER PREDICATE {schemaName}.fn_tenant_filter_{tableName}({tenantColumnName}) ON {schemaName}.{tableName},
            ADD BLOCK PREDICATE {schemaName}.fn_tenant_filter_{tableName}({tenantColumnName}) ON {schemaName}.{tableName}
            WITH (STATE = ON, SCHEMABINDING = ON);
            """;
    }

    /// <summary>
    /// Generates PostgreSQL RLS policy for a tenant-scoped table.
    /// Enables row-level security and creates a policy that restricts access
    /// to rows matching the tenant set via <c>current_setting('app.tenant_id')</c>.
    /// </summary>
    /// <param name="tableName">The table to protect.</param>
    /// <param name="tenantColumnName">The column that holds the tenant identifier. Defaults to "tenant_id".</param>
    /// <returns>SQL script that creates the RLS policy.</returns>
    public static string GeneratePostgreSqlPolicy(string tableName, string tenantColumnName = "tenant_id")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tableName);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantColumnName);

        return $"""
            -- Row-Level Security policy for {tableName}
            -- Ensures tenants can only access their own data at the database level

            -- 1. Enable RLS on table
            ALTER TABLE {tableName} ENABLE ROW LEVEL SECURITY;
            ALTER TABLE {tableName} FORCE ROW LEVEL SECURITY;

            -- 2. Create tenant isolation policy
            CREATE POLICY tenant_isolation_{tableName} ON {tableName}
            USING ({tenantColumnName}::text = current_setting('app.tenant_id', true))
            WITH CHECK ({tenantColumnName}::text = current_setting('app.tenant_id', true));

            -- 3. Grant access through policy (not directly)
            -- GRANT SELECT, INSERT, UPDATE, DELETE ON {tableName} TO app_user;
            """;
    }

    /// <summary>
    /// Generates SQL to set the tenant context for the current database session.
    /// Call this at the beginning of each request/connection to scope all subsequent
    /// queries to the specified tenant.
    /// </summary>
    /// <param name="provider">The database provider name: "sqlserver" or "postgresql".</param>
    /// <param name="tenantId">The tenant identifier to set for the session.</param>
    /// <returns>SQL statement that sets the tenant context.</returns>
    /// <exception cref="NotSupportedException">Thrown when the provider is not supported.</exception>
    public static string GenerateSetTenantContext(string provider, string tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(provider);
        ArgumentException.ThrowIfNullOrWhiteSpace(tenantId);

        return provider.ToLowerInvariant() switch
        {
            "sqlserver" => $"EXEC sp_set_session_context @key = N'TenantId', @value = '{tenantId}';",
            "postgresql" => $"SET app.tenant_id = '{tenantId}';",
            _ => throw new NotSupportedException($"RLS not supported for provider: {provider}")
        };
    }
}
