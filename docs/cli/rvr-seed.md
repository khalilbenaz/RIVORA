# rvr seed

Seed database with test/demo data using standardized `IRvrDataSeeder` implementations.

## Syntax

```bash
rvr seed [options]
rvr generate seed <entity> [options]
```

## Options

| Option | Description | Default |
|--------|-------------|---------|
| `--profile` | Seeding profile (dev, demo, test, perf) | `dev` |
| `--reset` | Truncate database before seeding | `false` |
| `--dry-run` | Show what would be seeded without executing | `false` |
| `--tenant` | Seed a specific tenant (multi-tenant) | all |

## IRvrDataSeeder Interface

```csharp
public interface IRvrDataSeeder
{
    string Profile { get; }  // "dev", "demo", "test", "perf"
    int Order { get; }       // Execution order (lower = first)
    Task SeedAsync(CancellationToken cancellationToken = default);
}
```

## Examples

```bash
# Seed with default profile (dev)
rvr seed

# Seed with demo profile
rvr seed --profile demo

# Reset database and reseed
rvr seed --reset --profile test

# Preview seeding
rvr seed --dry-run

# Multi-tenant seeding
rvr seed --tenant acme-corp

# Generate a seeder scaffold
rvr generate seed Product
rvr generate seed Product --profile demo
```

## Seeder Example

```csharp
public class ProductSeeder : IRvrDataSeeder
{
    public string Profile => "demo";
    public int Order => 10;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        // Seed logic here
    }
}
```
