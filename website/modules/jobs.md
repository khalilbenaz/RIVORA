# Module Jobs

**Packages** : `RVR.Framework.Jobs.Abstractions`, `RVR.Framework.Jobs.Hangfire`, `RVR.Framework.Jobs.Quartz`

## Description

Abstraction pour les background jobs avec 2 implementations : Hangfire et Quartz.NET.

## Enregistrement

### Hangfire

```csharp
builder.Services.AddRvrJobs(options =>
{
    options.Provider = JobProvider.Hangfire;
    options.StorageConnection = builder.Configuration.GetConnectionString("HangfireConnection");
    options.DashboardPath = "/hangfire";
});
```

### Quartz.NET

```csharp
builder.Services.AddRvrJobs(options =>
{
    options.Provider = JobProvider.Quartz;
    options.UsePersistentStore = true;
});
```

## Types de jobs

### Fire-and-forget

```csharp
await _jobService.EnqueueAsync<IEmailService>(
    svc => svc.SendWelcomeEmailAsync("user@example.com")
);
```

### Differe

```csharp
await _jobService.ScheduleAsync<IReportService>(
    svc => svc.GenerateMonthlyReport(),
    TimeSpan.FromHours(2)
);
```

### Recurrent

```csharp
_jobService.AddRecurring<ICleanupService>(
    "cleanup-expired-tokens",
    svc => svc.CleanExpiredTokensAsync(),
    Cron.Daily(3, 0) // Tous les jours a 3h
);
```

## Interface

```csharp
public interface IJobService
{
    Task<string> EnqueueAsync<T>(Expression<Func<T, Task>> action);
    Task<string> ScheduleAsync<T>(Expression<Func<T, Task>> action, TimeSpan delay);
    void AddRecurring<T>(string id, Expression<Func<T, Task>> action, string cron);
    void RemoveRecurring(string id);
}
```
