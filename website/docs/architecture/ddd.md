---
sidebar_position: 1
---

# 🏗️ Architecture & DDD Simplifié

KBA.Framework est basé sur la **Clean Architecture** et les principes du **Domain-Driven Design (DDD)**, mais sans la complexité habituelle.

## Les Couches de l'Application

La solution générée est divisée en 4 projets principaux :

1.  **`Domain`** : Le cœur de l'application. Contient les Entités (`Customer`, `Product`), les Value Objects et les interfaces de base. Ne dépend de rien d'autre.
2.  **`Application`** : Contient la logique métier (Use Cases). Utilise le pattern **CQRS** (Command Query Responsibility Segregation) avec MediatR.
3.  **`Infrastructure`** : Les détails techniques (Entity Framework Core, Redis, Hangfire, envois d'emails).
4.  **`Api`** : L'interface REST avec des **Minimal APIs** ultra-performantes.

## Entités et Aggregate Roots

Dans KBA, nous simplifions la création d'entités avec des classes de base puissantes :

```csharp
public class Product : AggregateRoot<Guid>, IMustHaveTenant, IAuditableEntity
{
    public string Name { get; private set; }
    public decimal Price { get; private set; }
    
    // Les champs TenantId, CreatedBy, CreatedAt, etc. sont gérés automatiquement !
}
```

*   `AggregateRoot<T>` : Entité principale qui émet des événements de domaine.
*   `IMustHaveTenant` : Indique au framework de filtrer automatiquement cette entité par `TenantId`.
*   `IAuditableEntity` : Le framework enregistre automatiquement qui a créé/modifié l'entité et quand.