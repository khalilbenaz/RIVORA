# Telecharger RVR Studio

RVR Studio Desktop est l'application officielle pour creer et gerer vos projets RIVORA.

## Installeurs

| Plateforme | Fichier | Format |
|------------|---------|--------|
| **Windows** (x64) | [RVR-Studio-Setup-win-x64.zip](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Setup-win-x64.zip) | ZIP |
| **macOS** (x64) | [RVR-Studio-Setup-macos.tar.gz](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Setup-macos.tar.gz) | tar.gz |
| **Linux** (x64) | [RVR-Studio-linux-x64.tar.gz](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-linux-x64.tar.gz) | tar.gz |

> Les installeurs sont attaches automatiquement a chaque [GitHub Release](https://github.com/khalilbenaz/RIVORA/releases).
>
> Prerequis : [.NET 9 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) installe sur la machine.

## Installation

### Windows
1. Telecharger `RVR-Studio-Setup-win-x64.zip`
2. Extraire dans un dossier
3. Lancer `install.bat` — installe dans `%LOCALAPPDATA%\RVR-Studio` et cree un raccourci Bureau
4. Double-cliquer le raccourci **RVR Studio** — ouvre `http://localhost:5200`

### macOS
1. Telecharger `RVR-Studio-Setup-macos.tar.gz`
2. Extraire : `tar -xzf RVR-Studio-Setup-macos.tar.gz -C ~/rvr-studio`
3. Lancer : `~/rvr-studio/rvr-studio.sh` — ouvre `http://localhost:5200`

### Linux
```bash
# Extraire
mkdir -p ~/rvr-studio
tar -xzf RVR-Studio-linux-x64.tar.gz -C ~/rvr-studio

# Lancer
~/rvr-studio/usr/bin/RVR.Studio.Desktop
```

## Alternative : RVR CLI

Si vous preferez la ligne de commande :

```bash
# Installer le CLI
dotnet tool install --global RVR.CLI

# Wizard interactif
rvr new
```

## Alternative : GitHub Codespaces

Pas d'installation requise — developpez directement dans le navigateur :

[![Open in GitHub Codespaces](https://github.com/codespaces/badge.svg)](https://codespaces.new/khalilbenaz/RIVORA)

## Fonctionnalites

- **Wizard de creation** : 10 etapes pour generer une solution complete
- **Dashboard local** : vue des projets RIVORA sur votre machine
- **Gestionnaire de modules** : ajout/suppression de modules en un clic
- **Terminal integre** : acces direct au RVR CLI
- **Mode hors ligne** : fonctionne sans connexion internet
- **Mise a jour automatique** : notifications de nouvelles versions
