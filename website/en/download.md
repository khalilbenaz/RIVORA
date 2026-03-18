# Download RVR Studio

RVR Studio Desktop is the official application to create and manage RIVORA projects.

## Installers

| Platform | File | Format |
|----------|------|--------|
| **Windows** (x64) | [RVR-Studio-Setup-win-x64.zip](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Setup-win-x64.zip) | ZIP |
| **macOS** (x64) | [RVR-Studio-Setup-macos.tar.gz](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Setup-macos.tar.gz) | tar.gz |
| **Linux** (x64) | [RVR-Studio-linux-x64.tar.gz](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-linux-x64.tar.gz) | tar.gz |

> Installers are automatically attached to each [GitHub Release](https://github.com/khalilbenaz/RIVORA/releases).
>
> Prerequisite: [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) installed on your machine.

## Installation

### Windows
1. Download `RVR-Studio-Setup-win-x64.zip`
2. Extract to a folder
3. Run `install.bat` — installs to `%LOCALAPPDATA%\RVR-Studio` and creates a Desktop shortcut
4. Double-click the **RVR Studio** shortcut — opens `http://localhost:5200`

### macOS
1. Download `RVR-Studio-Setup-macos.tar.gz`
2. Extract: `tar -xzf RVR-Studio-Setup-macos.tar.gz -C ~/rvr-studio`
3. Run: `~/rvr-studio/rvr-studio.sh` — opens `http://localhost:5200`

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
