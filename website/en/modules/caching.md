# Caching Module

**Package**: `RVR.Framework.Caching`

Two-level cache: Memory (L1) for speed, Redis (L2) for sharing across instances.

```csharp
builder.Services.AddRvrCaching(options =>
{
    options.EnableL1 = true;
    options.EnableL2 = true;
    options.RedisConnection = "localhost:6379";
});
```

See the [French documentation](/modules/caching) for detailed API reference.
