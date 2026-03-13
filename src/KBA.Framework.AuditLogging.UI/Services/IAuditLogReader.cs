namespace KBA.Framework.AuditLogging.UI.Services;
public interface IAuditLogReader
{
    Task<IEnumerable<object>> GetRecentLogsAsync(int count = 50);
}
