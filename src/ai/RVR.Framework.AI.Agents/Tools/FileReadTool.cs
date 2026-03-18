using Microsoft.Extensions.Logging;

namespace RVR.Framework.AI.Agents.Tools;

/// <summary>
/// A tool that reads file contents from the file system. Restricted to configured
/// base directories for security.
/// </summary>
public sealed class FileReadTool : ITool
{
    private readonly ILogger<FileReadTool> _logger;
    private readonly IReadOnlyList<string> _allowedBasePaths;
    private readonly long _maxFileSizeBytes;

    /// <summary>
    /// Initializes a new instance of <see cref="FileReadTool"/>.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="allowedBasePaths">
    /// The base directories that file reads are restricted to.
    /// If empty, only the current directory is allowed.
    /// </param>
    /// <param name="maxFileSizeBytes">Maximum file size in bytes. Defaults to 1 MB.</param>
    public FileReadTool(
        ILogger<FileReadTool> logger,
        IEnumerable<string>? allowedBasePaths = null,
        long maxFileSizeBytes = 1_048_576)
    {
        ArgumentNullException.ThrowIfNull(logger);

        _logger = logger;
        _allowedBasePaths = allowedBasePaths?.ToList() ?? [Directory.GetCurrentDirectory()];
        _maxFileSizeBytes = maxFileSizeBytes;
    }

    /// <inheritdoc />
    public string Name => "file_read";

    /// <inheritdoc />
    public string Description => "Reads the contents of a file at the specified path. Restricted to allowed directories.";

    /// <inheritdoc />
    public ToolSchema Schema => new(
        Name,
        Description,
        [
            new ToolParameter("path", "string", "The absolute or relative file path to read."),
            new ToolParameter("encoding", "string", "The file encoding (utf-8, ascii, etc.). Defaults to utf-8.", Required: false),
        ]);

    /// <inheritdoc />
    public async Task<ToolResult> ExecuteAsync(Dictionary<string, object> parameters, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(parameters);

        if (!parameters.TryGetValue("path", out var pathObj) || pathObj is not string path || string.IsNullOrWhiteSpace(path))
        {
            return new ToolResult(false, Error: "The 'path' parameter is required.");
        }

        var fullPath = Path.GetFullPath(path);

        // Security: ensure the path is under an allowed base directory
        if (!IsPathAllowed(fullPath))
        {
            _logger.LogWarning("FileReadTool access denied for path: {Path}", fullPath);
            return new ToolResult(false, Error: $"Access denied. The path '{fullPath}' is not within any allowed directory.");
        }

        if (!File.Exists(fullPath))
        {
            return new ToolResult(false, Error: $"File not found: {fullPath}");
        }

        _logger.LogInformation("FileReadTool reading file: {Path}", fullPath);

        try
        {
            var fileInfo = new FileInfo(fullPath);
            if (fileInfo.Length > _maxFileSizeBytes)
            {
                return new ToolResult(false, Error: $"File exceeds maximum size of {_maxFileSizeBytes:N0} bytes (actual: {fileInfo.Length:N0} bytes).");
            }

            var encodingName = parameters.TryGetValue("encoding", out var encObj) ? encObj.ToString() : "utf-8";
            var encoding = System.Text.Encoding.GetEncoding(encodingName ?? "utf-8");

            var content = await File.ReadAllTextAsync(fullPath, encoding, ct).ConfigureAwait(false);

            _logger.LogDebug("FileReadTool read {Length} chars from {Path}", content.Length, fullPath);
            return new ToolResult(true, Data: content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "FileReadTool failed to read {Path}", fullPath);
            return new ToolResult(false, Error: ex.Message);
        }
    }

    private bool IsPathAllowed(string fullPath)
    {
        return _allowedBasePaths.Any(basePath =>
            fullPath.StartsWith(Path.GetFullPath(basePath), StringComparison.OrdinalIgnoreCase));
    }
}
