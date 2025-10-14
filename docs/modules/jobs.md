# Jobs & Background Processing - RIVORA Framework

Système de jobs background avec support Hangfire et Quartz.NET.

## Table des Matières

- [Vue d'ensemble](#vue-densemble)
- [Hangfire](#hangfire)
- [Quartz.NET](#quartznet)
- [Job Abstractions](#job-abstractions)
- [Configuration](#configuration)
- [Dashboard](#dashboard)
- [Monitoring](#monitoring)

---

## Vue d'ensemble

RIVORA Framework fournit une abstraction unifiée pour les jobs background avec deux implémentations :

| Provider | Package | Use Case |
|----------|---------|----------|
| **Hangfire** | `RVR.Framework.Jobs.Hangfire` | Jobs simples, dashboard intégré |
| **Quartz.NET** | `RVR.Framework.Jobs.Quartz` | Scheduling avancé, cron expressions |

---

## Hangfire

### Installation

```bash
dotnet add package RVR.Framework.Jobs.Hangfire
```

### Configuration de base

```csharp
using RVR.Framework.Jobs.Hangfire.Extensions;

// Dans Program.cs
builder.Services.AddRvrHangfire(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    options =>
    {
        options.ServerName = "RVR-Server";
        options.WorkerCount = 10;
        options.Queues = new[] { "default", "critical", "low-priority" };
        options.DashboardPath = "/hangfire";
    });
```

### Configuration avancée

```csharp
builder.Services.AddRvrHangfire(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("Hangfire");
    options.DbProvider = "SqlServer"; // ou "PostgreSQL", "Redis"
    options.ServerName = "MyApp-Server";
    options.WorkerCount = 20;
    options.Queues = new[] { "default", "critical" };
    options.PollingIntervalSeconds = 15;
    options.SchemaName = "hangfire";
    options.DefaultMaxRetries = 3;
    options.DashboardPath = "/jobs";
    options.EnableDashboard = true;
    options.EnableServer = true;
});
```

### Configuration avec Redis

```csharp
builder.Services.AddRvrHangfire(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("Hangfire");
    options.DbProvider = "Redis";
    options.RedisConnectionString = builder.Configuration.GetConnectionString("Redis");
});
```

---

## Quartz.NET

### Installation

```bash
dotnet add package RVR.Framework.Jobs.Quartz
```

### Configuration de base

```csharp
using RVR.Framework.Jobs.Quartz.Extensions;

// Dans Program.cs
builder.Services.AddRvrQuartz(options =>
{
    options.ServerName = "RVR-Quartz";
    options.EnablePersistence = false; // In-memory
    options.DashboardPath = "/quartz";
});
```

### Configuration avec persistence database

```csharp
builder.Services.AddRvrQuartz(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    options =>
    {
        options.DbProvider = "SqlServer";
        options.TablePrefix = "QRTZ_";
        options.InstanceName = "RVR-Quartz-Instance";
        options.ThreadPoolSize = 10;
    });
```

### Configuration avancée

```csharp
builder.Services.AddRvrQuartz(options =>
{
    options.JobProvider = "Quartz";
    options.EnablePersistence = true;
    options.ConnectionString = builder.Configuration.GetConnectionString("Quartz");
    options.DbProvider = "SqlServer";
    options.TablePrefix = "QRTZ_";
    options.InstanceName = "MyApp-Quartz";
    options.InstanceId = "auto";
    options.ThreadPoolSize = 20;
    options.IdleWaitTime = TimeSpan.FromSeconds(30);
    options.DashboardPath = "/quartz-dashboard";
});
```

---

## Job Abstractions

### Interface IJobScheduler

```csharp
using RVR.Framework.Jobs.Abstractions.Interfaces;

public interface IJobScheduler
{
    Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> methodCall);
    Task<string> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    Task<string> ScheduleAsync<T>(Expression<Func<T, Task>> methodCall, DateTimeOffset enqueueAt);
    Task<string> RecurringAsync(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression);
    Task<bool> DeleteAsync(string jobId);
    Task<bool> PauseAsync(string jobId);
    Task<bool> ResumeAsync(string jobId);
}
```

### Exemple d'utilisation

```csharp
public class OrderService
{
    private readonly IJobScheduler _jobScheduler;

    public OrderService(IJobScheduler jobScheduler)
    {
        _jobScheduler = jobScheduler;
    }

    // Enqueue immediate
    public async Task ProcessOrderAsync(Order order)
    {
        await _jobScheduler.EnqueueAsync<OrderProcessingJob>(
            x => x.ExecuteAsync(order.Id));
    }

    // Schedule delayed
    public async Task ScheduleReminderAsync(Order order)
    {
        await _jobScheduler.ScheduleAsync<ReminderJob>(
            x => x.SendAsync(order.CustomerId),
            TimeSpan.FromHours(24));
    }

    // Recurring job
    public async Task SetupDailyReportAsync()
    {
        await _jobScheduler.RecurringAsync<DailyReportJob>(
            "daily-report",
            x => x.GenerateAsync(),
            "0 8 * * *"); // Every day at 8 AM
    }
}
```

---

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MyApp;Trusted_Connection=True;",
    "Hangfire": "Server=localhost;Database=Hangfire;Trusted_Connection=True;",
    "Quartz": "Server=localhost;Database=Quartz;Trusted_Connection=True;"
  },
  "JobSchedulerOptions": {
    "JobProvider": "Hangfire",
    "ServerName": "RVR-Server",
    "WorkerCount": 10,
    "Queues": ["default", "critical"],
    "PollingIntervalSeconds": 15,
    "DefaultMaxRetries": 3,
    "DashboardPath": "/hangfire",
    "EnableDashboard": true,
    "EnableServer": true
  }
}
```

### Options de configuration

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `JobProvider` | string | Hangfire | Provider (Hangfire/Quartz) |
| `ServerName` | string | RVR-Server | Nom du serveur |
| `WorkerCount` | int | 10 | Nombre de workers |
| `Queues` | string[] | default | Files d'attente |
| `PollingIntervalSeconds` | int | 15 | Intervalle de polling |
| `DefaultMaxRetries` | int | 3 | Nombre maximum de retries |
| `DashboardPath` | string | /hangfire | Chemin du dashboard |
| `EnableDashboard` | bool | true | Activer le dashboard |
| `EnableServer` | bool | true | Activer le processing server |
| `EnablePersistence` | bool | false | Persistence database (Quartz) |
| `ConnectionString` | string | - | Connection string |
| `DbProvider` | string | SqlServer | Database provider |

---

## Dashboard

### Hangfire Dashboard

```csharp
// Activer le dashboard
app.UseRvrHangfireDashboard();

// Dashboard avec authentification
app.UseRvrHangfireDashboard(new AllowAuthenticatedDashboardFilter());

// Dashboard avec rôles spécifiques
app.UseRvrHangfireDashboard(new RoleBasedDashboardFilter("Admin", "Operator"));
```

### Dashboard personnalisé

```csharp
app.UseHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization = new[] { new CustomAuthorizationFilter() },
    StatsPollingInterval = 5000,
    DisplayStorageConnectionString = false,
    IgnoreAntiforgeryToken = true
});
```

---

## Monitoring

### Health Checks

```csharp
// Ajouter health check pour les jobs
builder.Services.AddJobsHealthCheck(
    jobMonitor: sp => sp.GetRequiredService<IJobHealthMonitor>(),
    criticalQueues: new[] { "critical" });
```

### IJobHealthMonitor

```csharp
public interface IJobHealthMonitor
{
    Task<JobHealthStatus> GetStatusAsync();
    Task<IEnumerable<QueueStatus>> GetQueueStatusAsync();
    Task<IEnumerable<JobStatus>> GetFailedJobsAsync(int count = 10);
}

public class JobHealthStatus
{
    public bool IsHealthy { get; set; }
    public int ProcessingCount { get; set; }
    public int ScheduledCount { get; set; }
    public int FailedCount { get; set; }
    public int EnqueuedCount { get; set; }
}
```

---

## Exemples de Jobs

### Hangfire Job

```csharp
public class OrderProcessingJob
{
    private readonly ILogger<OrderProcessingJob> _logger;
    private readonly IOrderService _orderService;

    public OrderProcessingJob(
        ILogger<OrderProcessingJob> logger,
        IOrderService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task ExecuteAsync(int orderId)
    {
        _logger.LogInformation("Processing order {OrderId}", orderId);
        
        try
        {
            await _orderService.ProcessAsync(orderId);
            _logger.LogInformation("Order {OrderId} processed successfully", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process order {OrderId}", orderId);
            throw;
        }
    }
}
```

### Quartz Job

```csharp
public class DailyReportJob : IJob
{
    private readonly ILogger<DailyReportJob> _logger;
    private readonly IReportService _reportService;

    public DailyReportJob(
        ILogger<DailyReportJob> logger,
        IReportService reportService)
    {
        _logger = logger;
        _reportService = reportService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Generating daily report");
        
        try
        {
            await _reportService.GenerateDailyReportAsync();
            _logger.LogInformation("Daily report generated successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate daily report");
            throw new JobExecutionException(ex);
        }
    }
}

// Configuration du scheduling
var jobKey = new JobKey("daily-report", "reports");
var jobDetail = JobBuilder.Create<DailyReportJob>()
    .WithIdentity(jobKey)
    .Build();

var trigger = TriggerBuilder.Create()
    .WithIdentity("daily-report-trigger", "reports")
    .WithCronSchedule("0 8 * * *") // Every day at 8 AM
    .Build();

await scheduler.ScheduleJob(jobDetail, trigger);
```

---

## Bonnes Pratiques

### Retry Strategy

```csharp
// Hangfire - Retry avec backoff exponentiel
[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Delete)]
public async Task ExecuteAsync()
{
    // Job logic
}

// Quartz - Retry dans le job
public async Task Execute(IJobExecutionContext context)
{
    var retryCount = context.JobDetail.JobDataMap.GetInt("retry_count");
    
    try
    {
        // Job logic
    }
    catch (Exception ex) when (retryCount < 3)
    {
        context.JobDetail.JobDataMap.Put("retry_count", retryCount + 1);
        throw new JobExecutionException(ex, true); // true = reschedule
    }
}
```

### Job Filters

```csharp
// Logging filter
public class LoggingJobFilter : JobFilterAttribute, IServerFilter
{
    private readonly ILogger _logger;

    public void OnPerforming(PerformingContext context)
    {
        _logger.LogInformation("Starting job {JobId}", context.BackgroundJob.Id);
    }

    public void OnPerformed(PerformedContext context)
    {
        if (context.Exception != null)
        {
            _logger.LogError(context.Exception, "Job {JobId} failed", context.BackgroundJob.Id);
        }
        else
        {
            _logger.LogInformation("Job {JobId} completed successfully", context.BackgroundJob.Id);
        }
    }
}
```

---

## Voir aussi

- [Health Checks](health-checks.md) - Monitoring des jobs
- [Security](security.md) - Authorization du dashboard
- [Caching](caching.md) - Cache dans les jobs
