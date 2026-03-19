# RVR Studio Desktop v4.0

Cross-platform desktop application for RIVORA Framework, built with .NET MAUI Blazor Hybrid.

## Features

- Solution creation wizard (6 templates: SaaS, E-commerce, CRM, Blog, Internal, API)
- Entity Builder with full CRUD generation (8 files per entity)
- Domain Modeling with IA-assisted design (Claude, OpenAI, Ollama)
- Local project dashboard (scan and manage `.sln` files)
- Module manager (25+ modules: security, tenancy, billing, AI, etc.)
- Query Builder with Natural Language support
- Audit Logs viewer
- SaaS Subscriptions management
- File Storage browser
- Dynamic Translations editor
- Quick links to React Front End (Flow Builder, Kanban, Analytics, Chat)
- Integrated terminal for RVR CLI commands
- Offline-first — works without internet
- Auto-update via GitHub Releases

## Build

```bash
# Windows
dotnet build -f net9.0-windows10.0.19041.0

# macOS
dotnet build -f net9.0-maccatalyst

# Generic (Linux via Blazor Server fallback)
dotnet build -f net9.0
```

## Architecture

Uses MAUI Blazor Hybrid to reuse all Blazor components from `RVR.Studio`, providing a native desktop experience with a single codebase.

The React Front End (`frontend/`) provides additional pages (Flow Builder, Kanban, Chat, Analytics, Calendar, Notes) accessible via the browser at `http://localhost:3000`.

## Related

- [React Front End](../../frontend/) — 28 pages SPA
- [RVR CLI](../RVR.CLI/) — Command-line scaffolding and AI tools
- [Documentation](https://khalilbenaz.github.io/RIVORA/) — Full online docs
