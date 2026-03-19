# RVR CLI

The RIVORA CLI (`rvr`) is the primary tool for creating, managing, and maintaining RIVORA projects.

## Installation

```bash
dotnet tool install --global RVR.CLI
```

Verify the installation:

```bash
rvr --version
```

## Commands

| Command | Description |
|---------|-------------|
| [`rvr new`](/cli/new) | Create a new RIVORA project from a template |
| [`rvr generate`](/cli/generate) | Generate code (aggregates, CRUD, commands, queries, tests) |
| [`rvr ai`](/cli/ai) | AI-powered review, chat, generation, and design |
| [`rvr migrate`](/cli/migrate) | Database migration management |
| [`rvr env`](/cli/env) | Environment and secrets management |
| [`rvr publish`](/cli/publish) | Build and publish to Docker, NuGet, or Azure |
| [`rvr doctor`](/cli/doctor) | Run project diagnostics and health checks |
| [`rvr benchmark`](/cli/benchmark) | Performance and load testing |
| [`rvr add-module`](/cli/add-module) | Add a RIVORA module to your project |
| [`rvr remove-module`](/cli/remove-module) | Remove a module from your project |
| [`rvr upgrade`](/cli/upgrade) | Upgrade RIVORA to a newer version |

## Global Flags

| Flag | Description |
|------|-------------|
| `--help`, `-h` | Show help for any command |
| `--version` | Display CLI version |
| `--verbose`, `-v` | Enable verbose output |
| `--no-color` | Disable colored output |
| `--working-dir <path>` | Set the working directory |

## Templates

| Template | Description |
|----------|-------------|
| `saas-starter` | Full multi-tenant SaaS application |
| `api-minimal` | Minimal REST API |
| `microservices` | Microservices architecture with gateway |
| `ai-rag` | RAG application with vector store |
