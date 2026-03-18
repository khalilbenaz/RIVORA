# Telecharger RVR Studio

RVR Studio Desktop est l'application officielle pour creer et gerer vos projets RIVORA.

## Installeurs

| Plateforme | Fichier | Taille |
|------------|---------|--------|
| **Windows** (x64) | [RVR-Studio-Desktop-win-x64.zip](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Desktop-win-x64.zip) | ~50 MB |
| **macOS** (Universal) | [RVR-Studio-Desktop-macos.zip](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Desktop-macos.zip) | ~60 MB |
| **Linux** (x64) | [RVR-Studio-Desktop-linux-x64.tar.gz](https://github.com/khalilbenaz/RIVORA/releases/latest/download/RVR-Studio-Desktop-linux-x64.tar.gz) | ~55 MB |

> Les installeurs sont attaches automatiquement a chaque [GitHub Release](https://github.com/khalilbenaz/RIVORA/releases).

## Installation

### Windows
1. Telecharger le fichier `.zip`
2. Extraire dans un dossier
3. Lancer `RVR.Studio.Desktop.exe`

### macOS
1. Telecharger le fichier `.zip`
2. Extraire et deplacer l'app dans `/Applications`
3. Lancer l'application

### Linux
```bash
# Extraire
tar -xzf RVR-Studio-Desktop-linux-x64.tar.gz -C ~/rvr-studio

# Lancer
~/rvr-studio/RVR.Studio.Desktop
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
