namespace KBA.Framework.Security.Interceptors;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using KBA.Framework.Security.Entities;
using KBA.Framework.Security.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

/// <summary>
/// EF Core save changes interceptor that automatically creates audit trail entries
/// for Create, Update, and Delete operations.
/// </summary>
public class AuditTrailInterceptor : SaveChangesInterceptor
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditTrailInterceptor> _logger;
    private readonly AuditTrailInterceptorOptions _options;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    /// <summary>
    /// List to hold pending audit entries during save changes.
    /// </summary>
    private readonly List<AuditEntry> _pendingEntries = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="AuditTrailInterceptor"/> class.
    /// </summary>
    /// <param name="auditService">The audit service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The interceptor options.</param>
    /// <param name="httpContextAccessor">Optional HTTP context accessor for request info.</param>
    public AuditTrailInterceptor(
        IAuditService auditService,
        ILogger<AuditTrailInterceptor> logger,
        AuditTrailInterceptorOptions options,
        IHttpContextAccessor? httpContextAccessor = null)
    {
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc/>
    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        CaptureAuditEntries(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        CaptureAuditEntries(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <inheritdoc/>
    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        ProcessAuditEntries(eventData.Context).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    /// <inheritdoc/>
    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await ProcessAuditEntries(eventData.Context, cancellationToken);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void CaptureAuditEntries(DbContext? context)
    {
        if (context == null)
        {
            return;
        }

        _pendingEntries.Clear();

        var userId = GetUserId();
        var tenantId = GetTenantId();
        var ipAddress = GetIpAddress();
        var userAgent = GetUserAgent();
        var correlationId = GetCorrelationId();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditEntry)
            {
                // Don't audit audit entries
                continue;
            }

            if (!_options.ShouldAudit(entry.Entity.GetType()))
            {
                continue;
            }

            try
            {
                AuditEntry? auditEntry = entry.State switch
                {
                    EntityState.Added => CreateAuditEntryForCreate(entry, userId, tenantId, ipAddress, userAgent, correlationId),
                    EntityState.Modified => CreateAuditEntryForUpdate(entry, userId, tenantId, ipAddress, userAgent, correlationId),
                    EntityState.Deleted => CreateAuditEntryForDelete(entry, userId, tenantId, ipAddress, userAgent, correlationId),
                    _ => null
                };

                if (auditEntry != null)
                {
                    _pendingEntries.Add(auditEntry);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create audit entry for entity {EntityType}",
                    entry.Entity.GetType().Name);
            }
        }
    }

    private async Task ProcessAuditEntries(DbContext? context, CancellationToken cancellationToken = default)
    {
        if (context == null || _pendingEntries.Count == 0)
        {
            return;
        }

        try
        {
            foreach (var auditEntry in _pendingEntries)
            {
                await _auditService.LogAsync(auditEntry, cancellationToken);
            }

            _logger.LogDebug("Processed {Count} audit entries", _pendingEntries.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process audit entries");
        }
        finally
        {
            _pendingEntries.Clear();
        }
    }

    private AuditEntry? CreateAuditEntryForCreate(
        EntityEntry entry,
        string? userId,
        string? tenantId,
        string? ipAddress,
        string? userAgent,
        string? correlationId)
    {
        var entityType = entry.Entity.GetType().Name;
        var entityKey = GetEntityKey(entry);
        var newValues = GetValues(entry);

        var auditEntry = AuditEntry.CreateCreate(
            entityType,
            entityKey,
            newValues,
            userId,
            tenantId,
            ipAddress);

        auditEntry.UserAgent = userAgent;
        auditEntry.CorrelationId = correlationId;

        return auditEntry;
    }

    private AuditEntry? CreateAuditEntryForUpdate(
        EntityEntry entry,
        string? userId,
        string? tenantId,
        string? ipAddress,
        string? userAgent,
        string? correlationId)
    {
        var entityType = entry.Entity.GetType().Name;
        var entityKey = GetEntityKey(entry);

        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();
        var affectedColumns = new List<string>();

        foreach (var property in entry.Properties)
        {
            if (property.IsModified)
            {
                var propertyName = property.Metadata.Name;

                if (!_options.ShouldAuditProperty(entry.Entity.GetType(), propertyName))
                {
                    continue;
                }

                oldValues[propertyName] = property.OriginalValue;
                newValues[propertyName] = property.CurrentValue;
                affectedColumns.Add(propertyName);
            }
        }

        if (affectedColumns.Count == 0)
        {
            return null; // No meaningful changes
        }

        var auditEntry = AuditEntry.CreateUpdate(
            entityType,
            entityKey,
            Serialize(oldValues),
            Serialize(newValues),
            string.Join(",", affectedColumns),
            userId,
            tenantId,
            ipAddress);

        auditEntry.UserAgent = userAgent;
        auditEntry.CorrelationId = correlationId;

        return auditEntry;
    }

    private AuditEntry? CreateAuditEntryForDelete(
        EntityEntry entry,
        string? userId,
        string? tenantId,
        string? ipAddress,
        string? userAgent,
        string? correlationId)
    {
        var entityType = entry.Entity.GetType().Name;
        var entityKey = GetEntityKey(entry);
        var oldValues = GetValues(entry);

        var auditEntry = AuditEntry.CreateDelete(
            entityType,
            entityKey,
            oldValues,
            userId,
            tenantId,
            ipAddress);

        auditEntry.UserAgent = userAgent;
        auditEntry.CorrelationId = correlationId;

        return auditEntry;
    }

    private string GetEntityKey(EntityEntry entry)
    {
        var keyProperties = entry.Entity.GetType().GetProperties()
            .Where(p => p.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) ||
                       p.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), false).Any())
            .ToList();

        if (keyProperties.Count == 0)
        {
            return entry.Entity.GetHashCode().ToString();
        }

        var keyValues = keyProperties.Select(p => p.GetValue(entry.Entity)?.ToString()).Where(v => v != null);
        return string.Join("-", keyValues);
    }

    private string GetValues(EntityEntry entry)
    {
        var values = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            var propertyName = property.Metadata.Name;

            if (_options.ShouldAuditProperty(entry.Entity.GetType(), propertyName))
            {
                values[propertyName] = property.CurrentValue;
            }
        }

        return Serialize(values);
    }

    private static string Serialize(object? value)
    {
        if (value == null)
        {
            return string.Empty;
        }

        return JsonSerializer.Serialize(value, new JsonSerializerOptions
        {
            WriteIndented = false,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        });
    }

    private string? GetUserId()
    {
        if (_httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(c =>
                c.Type == System.Security.Claims.ClaimTypes.NameIdentifier ||
                c.Type == "sub" ||
                c.Type == "user_id")?.Value;
        }

        return null;
    }

    private string? GetTenantId()
    {
        if (_httpContextAccessor?.HttpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return _httpContextAccessor.HttpContext.User.FindFirst(c =>
                c.Type == "tenant_id" ||
                c.Type == "tid")?.Value;
        }

        return null;
    }

    private string? GetIpAddress()
    {
        if (_httpContextAccessor?.HttpContext != null)
        {
            var forwardedFor = _httpContextAccessor.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedFor))
            {
                return forwardedFor.Split(',')[0].Trim();
            }

            return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        return null;
    }

    private string? GetUserAgent()
    {
        return _httpContextAccessor?.HttpContext?.Request.Headers["User-Agent"].FirstOrDefault();
    }

    private string? GetCorrelationId()
    {
        return _httpContextAccessor?.HttpContext?.TraceIdentifier;
    }
}

/// <summary>
/// Configuration options for the audit trail interceptor.
/// </summary>
public class AuditTrailInterceptorOptions
{
    private readonly HashSet<Type> _excludedTypes = new();
    private readonly HashSet<string> _excludedProperties = new();
    private readonly HashSet<Type> _includedTypes = new();

    /// <summary>
    /// Gets or sets whether to audit all entities by default.
    /// If true, only excluded types are skipped.
    /// If false, only included types are audited.
    /// </summary>
    public bool AuditAllByDefault { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to audit create operations.
    /// </summary>
    public bool AuditCreates { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to audit update operations.
    /// </summary>
    public bool AuditUpdates { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to audit delete operations.
    /// </summary>
    public bool AuditDeletes { get; set; } = true;

    /// <summary>
    /// Excludes a type from auditing.
    /// </summary>
    /// <typeparam name="T">The entity type to exclude.</typeparam>
    /// <returns>The options instance for chaining.</returns>
    public AuditTrailInterceptorOptions ExcludeType<T>()
    {
        _excludedTypes.Add(typeof(T));
        return this;
    }

    /// <summary>
    /// Excludes a type from auditing.
    /// </summary>
    /// <param name="type">The entity type to exclude.</param>
    /// <returns>The options instance for chaining.</returns>
    public AuditTrailInterceptorOptions ExcludeType(Type type)
    {
        _excludedTypes.Add(type);
        return this;
    }

    /// <summary>
    /// Includes a type for auditing (when AuditAllByDefault is false).
    /// </summary>
    /// <typeparam name="T">The entity type to include.</typeparam>
    /// <returns>The options instance for chaining.</returns>
    public AuditTrailInterceptorOptions IncludeType<T>()
    {
        _includedTypes.Add(typeof(T));
        return this;
    }

    /// <summary>
    /// Excludes a property from auditing across all entities.
    /// </summary>
    /// <param name="propertyName">The property name to exclude.</param>
    /// <returns>The options instance for chaining.</returns>
    public AuditTrailInterceptorOptions ExcludeProperty(string propertyName)
    {
        _excludedProperties.Add(propertyName.ToLowerInvariant());
        return this;
    }

    /// <summary>
    /// Determines if a type should be audited.
    /// </summary>
    /// <param name="type">The entity type.</param>
    /// <returns>True if the type should be audited.</returns>
    public bool ShouldAudit(Type type)
    {
        if (_excludedTypes.Contains(type))
        {
            return false;
        }

        if (AuditAllByDefault)
        {
            return true;
        }

        return _includedTypes.Contains(type);
    }

    /// <summary>
    /// Determines if a property should be audited.
    /// </summary>
    /// <param name="entityType">The entity type.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>True if the property should be audited.</returns>
    public bool ShouldAuditProperty(Type entityType, string propertyName)
    {
        return !_excludedProperties.Contains(propertyName.ToLowerInvariant());
    }
}
