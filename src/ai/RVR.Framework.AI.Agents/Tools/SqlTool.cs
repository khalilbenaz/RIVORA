using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RVR.Framework.AI.Agents.Configuration;

namespace RVR.Framework.AI.Agents.Tools;

/// <summary>
/// A tool that executes read-only SQL queries against a configured database.
/// Write operations are blocked by default for safety.
/// </summary>
public sealed class SqlTool : ITool
{
    private readonly string _connectionString;
    private readonly ILogger<SqlTool> _logger;
    private readonly bool _allowWriteOperations;

    private static readonly string[] WriteKeywords =
        ["INSERT", "UPDATE", "DELETE", "DROP", "ALTER", "CREATE", "TRUNCATE", "EXEC", "EXECUTE", "MERGE"];

    /// <summary>
    /// Initializes a new instance of <see cref="SqlTool"/>.
    /// </summary>
    /// <param name="options">The agent options containing the connection string.</param>
    /// <param name="logger">The logger instance.</param>
    /// <param name="allowWriteOperations">Whether to allow write operations. Defaults to <c>false</c>.</param>
    public SqlTool(
        IOptions<SqlToolOptions> options,
        ILogger<SqlTool> logger,
        bool allowWriteOperations = false)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionString = options.Value.ConnectionString;
        _logger = logger;
        _allowWriteOperations = allowWriteOperations;
    }

    /// <inheritdoc />
    public string Name => "sql";

    /// <inheritdoc />
    public string Description => "Executes a read-only SQL query against the configured database and returns the results as JSON.";

    /// <inheritdoc />
    public ToolSchema Schema => new(
        Name,
        Description,
        [
            new ToolParameter("query", "string", "The SQL query to execute. Only SELECT statements are allowed by default."),
            new ToolParameter("maxRows", "integer", "Maximum number of rows to return. Defaults to 100.", Required: false),
        ]);

    /// <inheritdoc />
    public async Task<ToolResult> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (!parameters.TryGetValue("query", out var queryObj) || queryObj is not string query || string.IsNullOrWhiteSpace(query))
        {
            return new ToolResult(false, Error: "The 'query' parameter is required.");
        }

        if (!_allowWriteOperations && ContainsWriteOperation(query))
        {
            _logger.LogWarning("SqlTool blocked a write operation: {Query}", query[..Math.Min(query.Length, 100)]);
            return new ToolResult(false, Error: "Write operations are not allowed. Only SELECT queries are permitted.");
        }

        var maxRows = parameters.TryGetValue("maxRows", out var maxObj) && maxObj is int max ? max : 100;

        _logger.LogInformation("SqlTool executing query (length={Length}, maxRows={MaxRows})", query.Length, maxRows);

        try
        {
            await using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(ct).ConfigureAwait(false);

            await using var command = new SqlCommand(query, connection);
            command.CommandTimeout = 30;

            await using var reader = await command.ExecuteReaderAsync(ct).ConfigureAwait(false);

            var results = new List<Dictionary<string, object?>>();
            var rowCount = 0;

            while (await reader.ReadAsync(ct).ConfigureAwait(false) && rowCount < maxRows)
            {
                var row = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    var value = reader.IsDBNull(i) ? null : reader.GetValue(i);
                    row[reader.GetName(i)] = value;
                }
                results.Add(row);
                rowCount++;
            }

            var json = JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });

            _logger.LogDebug("SqlTool returned {RowCount} row(s)", rowCount);
            return new ToolResult(true, Data: json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SqlTool query failed");
            return new ToolResult(false, Error: ex.Message);
        }
    }

    private static bool ContainsWriteOperation(string query)
    {
        var normalized = query.Trim().ToUpperInvariant();
        return WriteKeywords.Any(keyword =>
            normalized.StartsWith(keyword, StringComparison.Ordinal) ||
            normalized.Contains($" {keyword} ", StringComparison.Ordinal));
    }
}

/// <summary>
/// Configuration options for the <see cref="SqlTool"/>.
/// </summary>
public sealed class SqlToolOptions
{
    /// <summary>
    /// The configuration section name.
    /// </summary>
    public const string SectionName = "AI:Agents:SqlTool";

    /// <summary>
    /// Gets or sets the database connection string.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;
}
