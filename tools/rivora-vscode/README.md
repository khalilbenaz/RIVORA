# RIVORA Framework - VS Code Extension

Official VS Code extension for the [RIVORA Framework](https://github.com/khalilbenaz/RVR.Framework). Provides snippets, commands, and productivity tools for building modular .NET applications with RIVORA.

## Installation

Install from the [VS Code Marketplace](https://marketplace.visualstudio.com/items?itemName=khalilbenaz.rivora-vscode) or search for **RIVORA Framework** in the Extensions panel.

**Prerequisite:** Install the `rvr` CLI tool first:

```bash
dotnet tool install -g RVR.CLI
```

## Commands

Open the Command Palette (`Ctrl+Shift+P` / `Cmd+Shift+P`) and type **RIVORA** to see all available commands.

| Command | Description |
|---------|-------------|
| RIVORA: New Solution | Scaffold a new RIVORA solution |
| RIVORA: Add Module | Add a module to the current solution |
| RIVORA: Remove Module | Remove a module from the solution |
| RIVORA: AI Review | Run AI-powered code review on the workspace |
| RIVORA: Generate Test | Generate tests using AI |
| RIVORA: Run Doctor | Diagnose project health and configuration |
| RIVORA: Migrate Database | Run pending EF Core migrations |
| RIVORA: Seed Data | Execute data seeders |
| RIVORA: Publish | Build and publish the application |
| RIVORA: Upgrade | Upgrade RIVORA packages to the latest version |
| RIVORA: List Environments | List configured environments |

## Snippets

Type any prefix below in a `.cs` file and press `Tab` to expand.

| Prefix | Description |
|--------|-------------|
| `rvr-entity` | DDD Entity with Id, CreatedAt, and factory method |
| `rvr-valueobject` | DDD Value Object |
| `rvr-handler` | MediatR Command/Query Handler |
| `rvr-domainevents` | Domain Event record |
| `rvr-endpoint` | Minimal API endpoint |
| `rvr-test-unit` | xUnit unit test class |
| `rvr-test-integration` | Integration test with WebApplicationFactory |
| `rvr-seeder` | IRvrDataSeeder implementation |
| `rvr-module` | Module registration pattern |

## Requirements

- VS Code 1.85.0 or later
- .NET 8.0+ SDK
- `rvr` CLI tool (`dotnet tool install -g RVR.CLI`)

## License

MIT
