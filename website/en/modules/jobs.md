# Jobs Module

**Packages**: `RVR.Framework.Jobs.Abstractions`, `RVR.Framework.Jobs.Hangfire`, `RVR.Framework.Jobs.Quartz`

Background job abstraction with Hangfire and Quartz.NET implementations.

```csharp
builder.Services.AddRvrJobs(options =>
{
    options.Provider = JobProvider.Hangfire;
    options.StorageConnection = connectionString;
});
```

See the [French documentation](/modules/jobs) for detailed API reference.
