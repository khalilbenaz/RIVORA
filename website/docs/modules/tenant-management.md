---
sidebar_position: 2
---

# 🏢 Gestion Multi-locataire (Tenant Management)

Le Multi-tenancy est au cœur de KBA.Framework. Il est conçu pour vous permettre de créer des applications SaaS (Software as a Service) sans avoir à vous soucier des fuites de données entre vos clients.

## Approches Supportées

KBA.Framework supporte les deux approches principales de l'isolation de données :

1.  **Isolation Logique (Single Database) :** Tous les locataires partagent la même base de données. Les entités ont une colonne `TenantId`. C'est l'approche la plus économique.
2.  **Isolation Physique (Database per Tenant) :** Chaque locataire possède sa propre base de données physique. KBA s'occupe de changer dynamiquement la chaîne de connexion selon le locataire appelant.

## Comment l'activer ?

Il suffit de faire hériter votre entité de l'interface `IMustHaveTenant` :

```csharp
public class Product : Entity<Guid>, IMustHaveTenant
{
    public string Name { get; set; }
    
    // Le Framework ajoutera et gérera ce champ
    public string TenantId { get; set; } 
}
```

**C'est tout.** Entity Framework Core est automatiquement configuré pour ajouter un *Query Filter* global. 

*   Si vous faites `dbContext.Products.ToList()`, le framework ne retournera **que** les produits du Tenant actuel (déduit via le Header HTTP `X-Tenant-Id` ou le JWT Token).
*   Si vous insérez un produit, le `TenantId` est rempli automatiquement.
