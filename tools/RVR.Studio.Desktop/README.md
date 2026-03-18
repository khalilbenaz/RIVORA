# RVR Studio Desktop

Cross-platform desktop application for RIVORA Framework, built with .NET MAUI Blazor Hybrid.

## Features

- Solution creation wizard (shared with RVR Studio web)
- Local project dashboard (scan and manage `.sln` files)
- Module manager (add/remove RIVORA modules)
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

Uses MAUI Blazor Hybrid to reuse all Blazor components from `RVR.Framework.Admin`, providing a native desktop experience with a single codebase.
