# AuditLogging.UI

The RIVORA Framework captures detailed audit logs for every API request and entity change. The AuditLogging.UI module provides a visual interface for viewing, filtering, and exporting these logs.

## Audit Log Entity

Each audit entry captures:

```csharp
public class AuditLog : Entity<Guid>
{
    public Guid? TenantId { get; }
    public Guid? UserId { get; }
    public string? IpAddress { get; }
    public string? BrowserInfo { get; }
    public string? HttpMethod { get; }       // GET, POST, PUT, DELETE
    public string? Url { get; }
    public int? HttpStatusCode { get; }
    public int ExecutionTime { get; }         // Milliseconds
    public string? ExceptionMessage { get; }
    public DateTime ExecutionDate { get; }

    // Related data
    public ICollection<AuditLogAction> Actions { get; }
    public ICollection<EntityChange> EntityChanges { get; }
}
```

## Timeline View

The UI module renders audit logs in a timeline view, showing:

- Request details (method, URL, status code)
- User information (who performed the action)
- Execution time (with performance highlighting)
- Entity changes (before/after values)
- Exception details (if any)

## Filters

Filter audit logs by:

| Filter | Example |
|--------|---------|
| Date range | Last 24h, last 7 days, custom range |
| User | Filter by user ID or username |
| Tenant | Filter by tenant ID |
| HTTP method | GET, POST, PUT, DELETE |
| Status code | 200, 404, 500 |
| URL pattern | `/api/products/*` |
| Execution time | Slow requests (> 1000ms) |
| Has errors | Show only failed requests |

## Export

Export audit logs in multiple formats, integrating with the Export module:

```csharp
// Programmatic export
var logs = await _auditLogService.GetLogsAsync(new AuditLogFilter
{
    StartDate = DateTime.UtcNow.AddDays(-30),
    EndDate = DateTime.UtcNow,
    HttpMethods = new[] { "POST", "PUT", "DELETE" }
});

// Export to PDF
await _exportService.ExportToPdfAsync(logs, outputStream);

// Export to Excel
await _exportService.ExportToExcelAsync(logs, outputStream);

// Export to CSV
await _exportService.ExportToCsvAsync(logs, outputStream);
```

## Registration

```csharp
builder.Services.AddRvrAuditLogging(options =>
{
    options.IsEnabled = true;
    options.LogRequestBody = false;     // Privacy: do not log request bodies
    options.LogResponseBody = false;
    options.IgnoredUrls = new[] { "/health", "/health/ready", "/swagger" };
    options.SlowRequestThresholdMs = 1000;
});

// Add the UI module (Blazor-based)
builder.Services.AddRvrAuditLoggingUI();
```

## Middleware Integration

The audit logging middleware automatically captures:

```csharp
// In Program.cs
app.UseRvrAuditLogging();  // Captures all HTTP requests

// Or use selective auditing
app.UseRvrAuditLogging(options =>
{
    options.IncludePaths = new[] { "/api/" };
    options.ExcludePaths = new[] { "/api/health", "/swagger" };
});
```

## Querying Audit Logs Programmatically

```csharp
public class AuditService
{
    private readonly IAuditLogRepository _repository;

    public async Task<PagedResult<AuditLog>> GetUserActivityAsync(
        Guid userId, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await _repository.GetPagedAsync(
            filter: log => log.UserId == userId,
            orderBy: log => log.ExecutionDate,
            descending: true,
            page: page,
            pageSize: pageSize,
            ct: ct);
    }

    public async Task<IReadOnlyList<EntityChange>> GetEntityHistoryAsync(
        string entityType, string entityId, CancellationToken ct = default)
    {
        return await _repository.GetEntityChangesAsync(entityType, entityId, ct);
    }
}
```

## Entity Change Tracking

The framework automatically tracks property-level changes for audited entities:

```csharp
public class EntityChange
{
    public string EntityTypeFullName { get; set; }
    public string EntityId { get; set; }
    public ChangeType ChangeType { get; set; }  // Created, Updated, Deleted
    public ICollection<EntityPropertyChange> PropertyChanges { get; set; }
}

public class EntityPropertyChange
{
    public string PropertyName { get; set; }
    public string? OriginalValue { get; set; }
    public string? NewValue { get; set; }
}
```
