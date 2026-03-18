# Download RVR Studio

RVR Studio Desktop is the official application to create and manage RIVORA projects.

## Installers

| Platform | File | Size |
|----------|------|------|
| **Windows** (x64) | [RVR-Studio-Desktop-win-x64.zip](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Desktop-win-x64.zip) | ~50 MB |
| **macOS** (Universal) | [RVR-Studio-Desktop-macos.zip](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Desktop-macos.zip) | ~60 MB |
| **Linux** (x64) | [RVR-Studio-Desktop-linux-x64.tar.gz](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Desktop-linux-x64.tar.gz) | ~55 MB |

> Installers are automatically attached to each [GitHub Release](https://github.com/khalilbenaz/RIVORA/releases).

## Installation

### Windows
1. Download the `.zip` file
2. Extract to a folder
3. Run `RVR.Studio.Desktop.exe`

### macOS
1. Download the `.zip` file
2. Extract and move the app to `/Applications`
3. Launch the application

### Linux
```bash
# Extract
tar -xzf RVR-Studio-Desktop-linux-x64.tar.gz -C ~/rvr-studio

# Run
~/rvr-studio/RVR.Studio.Desktop
```

## Alternative: RVR CLI

If you prefer the command line:

```bash
# Install the CLI
dotnet tool install --global RVR.CLI

# Interactive wizard
rvr new
```

## Alternative: GitHub Codespaces

No installation required — develop directly in the browser:

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/khalilbenaz/RIVORA)

## Features

- **Creation wizard**: 10-step guided solution creation
- **Local dashboard**: view RIVORA projects on your machine
- **Module manager**: add/remove modules with one click
- **Integrated terminal**: direct access to RVR CLI
- **Offline mode**: works without internet
- **Auto-update**: new version notifications
