namespace RVR.Framework.Domain.Entities.BackgroundJobs;

/// <summary>
/// Tâche en arrière-plan
/// </summary>
public class BackgroundJob : Entity<Guid>
{
    /// <summary>
    /// Nom de la tâche
    /// </summary>
    public string JobName { get; private set; } = string.Empty;

    /// <summary>
    /// Arguments de la tâche (JSON)
    /// </summary>
    public string? JobArgs { get; private set; }

    /// <summary>
    /// Nombre de tentatives
    /// </summary>
    public int TryCount { get; private set; }

    /// <summary>
    /// Date et heure de la prochaine tentative
    /// </summary>
    public DateTime NextTryTime { get; private set; }

    /// <summary>
    /// Date et heure de la dernière tentative
    /// </summary>
    public DateTime? LastTryTime { get; private set; }

    /// <summary>
    /// Indique si la tâche est abandonnée
    /// </summary>
    public bool IsAbandoned { get; private set; }

    /// <summary>
    /// Priorité de la tâche
    /// </summary>
    public int Priority { get; private set; }

    /// <summary>
    /// Date de création
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// Constructeur privé pour EF Core
    /// </summary>
    private BackgroundJob() { }

    /// <summary>
    /// Crée une nouvelle tâche en arrière-plan
    /// </summary>
    /// <param name="jobName">Nom de la tâche</param>
    /// <param name="jobArgs">Arguments de la tâche</param>
    /// <param name="priority">Priorité</param>
    public BackgroundJob(string jobName, string? jobArgs = null, int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Le nom de la tâche ne peut pas être vide.", nameof(jobName));

        Id = Guid.NewGuid();
        JobName = jobName.Trim();
        JobArgs = jobArgs?.Trim();
        Priority = priority;
        TryCount = 0;
        NextTryTime = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
        IsAbandoned = false;
    }

    /// <summary>
    /// Enregistre une tentative d'exécution
    /// </summary>
    public void RecordTry()
    {
        TryCount++;
        LastTryTime = DateTime.UtcNow;
        NextTryTime = DateTime.UtcNow.AddMinutes(Math.Pow(2, TryCount)); // Exponential backoff
    }

    /// <summary>
    /// Abandonne la tâche
    /// </summary>
    public void Abandon()
    {
        IsAbandoned = true;
    }
}
