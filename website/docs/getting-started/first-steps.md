# Tutoriel Pas-à-Pas

Bienvenue dans ce tutoriel pratique ! Nous allons créer votre premier module métier en utilisant les outils visuels de **KBA Studio**.

## Objectif
Créer un système de **Gestion de Stock (StockItem)** complet (Entité, DTOs, API, Base de données) en moins de 10 minutes.

---

## 1. Lancer KBA Studio

1. Ouvrez votre solution dans votre IDE préféré.
2. Lancez le projet **KBA.Studio** (dossier `tools/`).
3. L'interface s'ouvrira sur `http://localhost:5200`.

---

## 2. Créer une Nouvelle Solution

Si vous démarrez de zéro :
1. Cliquez sur **🚀 Solution Creator**.
2. Nommez votre projet (ex: `MyStore`).
3. Sélectionnez l'architecture **🏢 Monolithe Modulaire**.
4. Cliquez sur **🚀 CRÉER LA SOLUTION**.

---

## 3. Créer l'Entité StockItem

Allez dans **🛠️ Entity Builder** :

### Propriétés
- **Nom** : `StockItem`
- **Options** : Activez `Aggregate Root`, `Multitenancy` et `Audit`.

### Champs
Ajoutez les champs :
- `SKU` (string, Required, 20)
- `Name` (string, Required, 255)
- `Quantity` (int, Required)
- `Price` (decimal, Required)

### Action
Cliquez sur **💾 Générer PHYSIQUEMENT les fichiers**.

---

## 4. Migration Base de Données

Dans la section **Database Migrations** :
1. Entrez `Initial_Stock` comme nom de migration.
2. Cliquez sur **➕ Créer la Migration**.
3. Une fois terminée, cliquez sur **🔄 Mettre à jour la DB**.

---

## 5. Test de l'API

Lancez votre API (`src/KBA.Framework.Api`) et ouvrez Swagger. Votre nouveau controller `StockItems` est prêt à l'emploi ! 🎉

---

## Prochaines étapes

Maintenant que vous avez la structure, vous pouvez :
- Ajouter de la logique métier dans `StockItem.cs`.
- Personnaliser les validators dans la couche Application.
- Créer une interface utilisateur pour consommer l'API.
