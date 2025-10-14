namespace RVR.Framework.AuditLogging.UI.Models;

/// <summary>
/// View model for displaying an audit log entry in the UI dashboard.
/// </summary>
public class AuditEntryViewModel
{
    public Guid Id { get; set; }
    public DateTime ExecutionDate { get; set; }
    public Guid? UserId { get; set; }
    public string? UserDisplay => UserId?.ToString("N")[..8] ?? "System";
    public string? IpAddress { get; set; }
    public string? HttpMethod { get; set; }
    public string? Url { get; set; }
    public int? HttpStatusCode { get; set; }
    public int ExecutionTime { get; set; }
    public string? ExceptionMessage { get; set; }
    public string? EntityType { get; set; }
    public string? Action { get; set; }

    public bool IsError => HttpStatusCode is >= 400;
    public bool IsSuccess => HttpStatusCode is >= 200 and < 400;
}

/// <summary>
/// Filter criteria for querying audit log entries.
/// </summary>
public class AuditLogFilter
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? UserId { get; set; }
    public string? EntityType { get; set; }
    public string? Action { get; set; }
    public string? SearchTerm { get; set; }
    public string? StatusFilter { get; set; }
}
