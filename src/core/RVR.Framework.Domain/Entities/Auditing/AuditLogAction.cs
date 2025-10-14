namespace RVR.Framework.Domain.Entities.Auditing;

/// <summary>
/// Action spécifique dans un log d'audit (appel de méthode de service)
/// </summary>
public class AuditLogAction : Entity<Guid>
{
    /// <summary>
    /// Identifiant du log d'audit parent
    /// </summary>
    public Guid AuditLogId { get; private set; }

    /// <summary>
    /// Nom du service
    /// </summary>
    public string? ServiceName { get; private set; }

    /// <summary>
    /// Nom de la méthode
    /// </summary>
    public string? MethodName { get; private set; }

    /// <summary>
    /// Paramètres de la méthode (JSON)
    /// </summary>
    public string? Parameters { get; private set; }

    /// <summary>
    /// Durée d'exécution en millisecondes
    /// </summary>
    public int ExecutionDuration { get; private set; }

    /// <summary>
    /// Log d'audit parent
    /// </summary>
    public virtual AuditLog AuditLog { get; private set; } = null!;

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private AuditLogAction() { }

    /// <summary>
    /// Crée une nouvelle action d'audit
    /// </summary>
    /// <param name="auditLogId">Identifiant du log d'audit</param>
    /// <param name="serviceName">Nom du service</param>
    /// <param name="methodName">Nom de la méthode</param>
    /// <param name="parameters">Paramètres</param>
    /// <param name="executionDuration">Durée d'exécution</param>
    public AuditLogAction(Guid auditLogId, string? serviceName, string? methodName, string? parameters, int executionDuration)
    {
        if (auditLogId == Guid.Empty)
            throw new ArgumentException("L'identifiant du log d'audit ne peut pas être vide.", nameof(auditLogId));

        Id = Guid.NewGuid();
        AuditLogId = auditLogId;
        ServiceName = serviceName?.Trim();
        MethodName = methodName?.Trim();
        Parameters = parameters?.Trim();
        ExecutionDuration = executionDuration;
    }
}
