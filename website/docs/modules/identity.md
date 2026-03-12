---
sidebar_position: 1
---

# 🔐 Module Identité (Authentication & Authorization)

KBA.Framework est livré avec un système de gestion d'identité robuste, pensé pour le B2B et le Multi-tenancy, s'appuyant sur les standards de l'industrie (JWT, OpenID Connect).

## Fonctionnalités

*   **Authentification JWT :** Les API sont protégées via des tokens JWT signés.
*   **Permissions Granulaires :** Oubliez les "Roles" simples. Le framework permet d'assigner des permissions spécifiques (`Invoices.Create`, `Invoices.View`) à des rôles.
*   **Isolation Multi-locataire :** L'utilisateur "Admin" du Locataire A n'est pas le même que l'utilisateur "Admin" du Locataire B.
*   **Refresh Tokens :** Gestion sécurisée des sessions longues.

## Utilisation

Protégez un Endpoint d'API en spécifiant la permission requise via notre attribut personnalisé `[RequirePermission]` :

```csharp
[ApiController]
[Route("api/[controller]")]
public class InvoicesController : ControllerBase
{
    [HttpPost]
    [RequirePermission("Permissions.Invoices.Create")]
    public async Task<IActionResult> CreateInvoice(CreateInvoiceDto request)
    {
        // ...
    }
}
```

Le framework vérifiera automatiquement que :
1. L'utilisateur est connecté.
2. L'utilisateur appartient au bon locataire (Tenant).
3. L'utilisateur possède le rôle ayant la permission `Permissions.Invoices.Create`.
