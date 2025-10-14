# 🎓 Tutoriel Pas-à-Pas : Vos Premiers Pas avec RVR Studio

Ce guide vous accompagne dans la création de votre premier module métier en utilisant les outils visuels du **RVR Studio**. Nous allons créer un système simple de **Gestion de Stock (StockItem)** de A à Z.

---

## 1. Lancer RVR Studio

Pour commencer, ouvrez la solution `RVR.Framework.sln` dans Visual Studio ou VS Code et lancez le projet **RVR.Studio** situé dans le dossier `tools/`.

L'interface s'ouvrira dans votre navigateur sur `http://localhost:5200` (ou port similaire).

---

## 2. Créer une Nouvelle Solution

Si vous ne travaillez pas déjà sur un projet existant, utilisez le **Solution Creator** :

1. Cliquez sur **🚀 Solution Creator** dans le menu latéral.
2. Donnez un nom à votre projet (ex: `MagasinEntreprise`).
3. Choisissez l'architecture **🏢 Monolithe Modulaire** (recommandé pour débuter).
4. Sélectionnez le template **SaaS Starter**.
5. Cliquez sur **🚀 CRÉER LA SOLUTION**.

*Le RVR CLI va générer toute la structure Clean Architecture pour vous dans le dossier spécifié.*

---

## 3. Concevoir l'Entité métier (Visual Builder)

Allez maintenant dans l'**🛠️ Entity Builder** pour créer votre entité `StockItem`.

### Configuration de l'entité :
- **Nom** : `StockItem`
- **Options** : Cochez `Aggregate Root`, `Support Multitenant` et `IAuditableEntity`.

### Ajout des champs :
Ajoutez les propriétés suivantes :
1. `SKU` (string, Requis, Max 20)
2. `DisplayName` (string, Requis, Max 255)
3. `Quantity` (int, Requis)
4. `Price` (decimal, Requis)

### Génération du code :
1. Cliquez sur **🔍 Prévisualiser** pour voir le code généré (Entity, DTOs, Configuration, Controller).
2. Cliquez sur **💾 Générer PHYSIQUEMENT les fichiers**.

*RVR Studio vient d'écrire 4 nouveaux fichiers dans votre solution `src/`.*

---

## 4. Gérer la Base de Données (Migrations)

Toujours sur la page de l'Entity Builder, descendez jusqu'à la section **Database Migrations** :

1. Dans **Nom de la Migration**, saisissez `AddStockItemTable`.
2. Cliquez sur **➕ Créer la Migration**.
3. Attendez le message de succès ✅.
4. Cliquez sur **🔄 Mettre à jour la DB** pour créer la table physiquement dans SQL Server.

---

## 5. Tester l'API

Votre nouveau module est maintenant prêt et exposé via une API REST !

1. Lancez votre projet API (`src/RVR.Framework.Api`).
2. Accédez au **Swagger UI** (`/swagger`).
3. Vous verrez un nouveau groupe **StockItems**.
4. Testez le `POST /api/StockItems` pour ajouter votre premier article.

---

## 🚀 Prochaines étapes

- **Validation** : Ajoutez des règles de validation personnalisées dans `src/RVR.Framework.Application/Validators/StockItemValidator.cs`.
- **Business Logic** : Implémentez des méthodes métier dans votre entité `StockItem.cs` (ex: `AddStock`, `RemoveStock`).
- **UI** : Créez une page Blazor ou Angular pour consommer cette nouvelle API.

---

**Bravo !** Vous avez créé un module complet avec base de données et API en moins de 10 minutes grâce au RIVORA Framework.
