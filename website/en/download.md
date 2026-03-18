# Download RVR Studio

RVR Studio Desktop is the official application to create and manage RIVORA projects.

## Installers

| Platform | File | Format |
|----------|------|--------|
| **Windows** (x64) | [RVR-Studio-Setup-win-x64.zip](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Setup-win-x64.zip) | ZIP |
| **macOS** | [RVR-Studio-Setup-macos.dmg](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Setup-macos.dmg) | DMG |
| **Linux** (x64) | [RVR-Studio-linux-x64.tar.gz](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-linux-x64.tar.gz) | tar.gz |

> Installers are automatically attached to each [GitHub Release](https://github.com/khalilbenaz/RIVORA/releases).

## Installation

### Windows
1. Download `RVR-Studio-Setup-win-x64.zip`
2. Extract to a folder
3. Run `RVR.Studio.Desktop.exe` — opens `http://localhost:5200` automatically

### macOS
1. Download `RVR-Studio-Setup-macos.dmg`
2. Open the DMG and drag RVR Studio to `/Applications`
3. Launch the app

### Linux
```bash
# Extract
mkdir -p ~/rvr-studio
tar -xzf RVR-Studio-linux-x64.tar.gz -C ~/rvr-studio

# Run
~/rvr-studio/usr/bin/RVR.Studio.Desktop
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
