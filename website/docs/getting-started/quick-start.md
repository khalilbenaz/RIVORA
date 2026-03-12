---
sidebar_position: 1
---

# ⏱️ Démarrage Rapide

Ce guide vous montrera comment installer KBA.Framework et lancer votre première application.

## 1. Prérequis

Assurez-vous d'avoir installé :
*   [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
*   [Node.js](https://nodejs.org/) (pour les outils Front-end)
*   Docker (Optionnel, mais recommandé pour les bases de données)

## 2. Installation du CLI KBA

La manière la plus simple de démarrer est d'utiliser notre outil en ligne de commande (CLI).

```bash
dotnet tool install -g KBA.CLI
```

## 3. Création de votre projet

Une fois le CLI installé, générez un nouveau projet SaaS complet avec la commande suivante :

```bash
kba new MaSuperApp --template saas-starter --tenancy row
```

Cela va générer une solution complète basée sur la Clean Architecture avec le support Multitenant pré-configuré.

## 4. Démarrage de KBA Studio

Dans votre nouveau projet, vous trouverez l'outil **KBA Studio**, une interface visuelle pour accélérer votre développement.

```bash
cd tools/KBA.Studio
dotnet run
```
Ouvrez votre navigateur et commencez à créer vos entités visuellement !