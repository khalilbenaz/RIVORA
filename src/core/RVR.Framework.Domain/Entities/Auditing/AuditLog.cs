namespace RVR.Framework.Domain.Entities.Auditing;

/// <summary>
/// Journal d'audit pour tracer toutes les opérations
/// </summary>
public class AuditLog : Entity<Guid>
{
    /// <summary>
    /// Identifiant du tenant
    /// </summary>
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// Identifiant de l'utilisateur
    /// </summary>
    public Guid? UserId { get; private set; }

    /// <summary>
    /// Adresse IP du client
    /// </summary>
    public string? IpAddress { get; private set; }

    /// <summary>
    /// Navigateur / User Agent
    /// </summary>
    public string? BrowserInfo { get; private set; }

    /// <summary>
    /// Méthode HTTP (GET, POST, PUT, DELETE, etc.)
    /// </summary>
    public string? HttpMethod { get; private set; }

    /// <summary>
    /// URL appelée
    /// </summary>
    public string? Url { get; private set; }

    /// <summary>
    /// Code de statut HTTP de la réponse
    /// </summary>
    public int? HttpStatusCode { get; private set; }

    /// <summary>
    /// Durée d'exécution en millisecondes
    /// </summary>
    public int ExecutionTime { get; private set; }

    /// <summary>
    /// Message d'exception si une erreur s'est produite
    /// </summary>
    public string? ExceptionMessage { get; private set; }

    /// <summary>
    /// Date et heure de l'opération
    /// </summary>
    public DateTime ExecutionDate { get; private set; }

    /// <summary>
    /// Actions liées à ce log d'audit
    /// </summary>
    public virtual ICollection<AuditLogAction> Actions { get; private set; } = new List<AuditLogAction>();

    /// <summary>
    /// Changements d'entités liés à ce log d'audit
    /// </summary>
    public virtual ICollection<EntityChange> EntityChanges { get; private set; } = new List<EntityChange>();

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private AuditLog() { }

    /// <summary>
    /// Crée un nouveau log d'audit
    /// </summary>
    /// <param name="tenantId">Identifiant du tenant</param>
    /// <param name="userId">Identifiant de l'utilisateur</param>
    /// <param name="ipAddress">Adresse IP</param>
    /// <param name="httpMethod">Méthode HTTP</param>
    /// <param name="url">URL</param>
    public AuditLog(Guid? tenantId, Guid? userId, string? ipAddress, string? httpMethod, string? url)
    {
        Id = Guid.NewGuid();
        TenantId = tenantId;
        UserId = userId;
        IpAddress = ipAddress?.Trim();
        HttpMethod = httpMethod?.Trim();
        Url = url?.Trim();
        ExecutionDate = DateTime.UtcNow;
    }

    /// <summary>
    /// Définit les informations du navigateur
    /// </summary>
    public void SetBrowserInfo(string? browserInfo)
    {
        BrowserInfo = browserInfo?.Trim();
    }

    /// <summary>
    /// Définit le résultat de l'exécution
    /// </summary>
    public void SetExecutionResult(int statusCode, int executionTime, string? exceptionMessage = null)
    {
        HttpStatusCode = statusCode;
        ExecutionTime = executionTime;
        ExceptionMessage = exceptionMessage?.Trim();
    }
}
