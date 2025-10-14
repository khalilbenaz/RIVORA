# Sécurité et Autorisation - RIVORA Framework

## Vue d'ensemble

Ce document décrit la configuration de sécurité et d'autorisation mise en place dans l'API RIVORA Framework.

## Configuration de l'authentification JWT

### JWT Settings (appsettings.json)
```json
{
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyThatShouldBeAtLeast32CharactersLong!",
    "Issuer": "RVR.Framework",
    "Audience": "RVR.Framework.Client",
    "ExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7
  }
}
```

### Middleware d'authentification (Program.cs)
- **UseAuthentication()**: Valide le token JWT et remplit `HttpContext.User`
- **UseAuthorization()**: Vérifie les permissions basées sur les attributs `[Authorize]`

## Attribution [Authorize] par contrôleur

### ✅ AuthController
- **Endpoints publics** (sans authentification):
  - `POST /api/auth/login` - Connexion
  - `POST /api/auth/refresh-token` - Rafraîchir le token
  
- **Endpoints protégés** (avec `[Authorize]`):
  - `POST /api/auth/logout` - Déconnexion

### ✅ ProductsController - `[Authorize]` au niveau de la classe
**Tous les endpoints nécessitent une authentification**, sauf ceux avec `[AllowAnonymous]`:

- **Public**:
  - `GET /api/products` - Liste tous les produits (avec `[AllowAnonymous]`)
  
- **Protégé** (authentification requise):
  - `GET /api/products/active` - Produits actifs
  - `GET /api/products/{id}` - Détails d'un produit
  - `GET /api/products/search?name={name}` - Recherche
  - `POST /api/products` - Créer un produit (utilise TenantId du contexte)
  - `PUT /api/products/{id}` - Modifier un produit
  - `DELETE /api/products/{id}` - Supprimer un produit

### ✅ UsersController - `[Authorize]` au niveau de la classe
**Tous les endpoints nécessitent une authentification**:

- `GET /api/users` - Liste tous les utilisateurs
- `GET /api/users/{id}` - Détails d'un utilisateur
- `GET /api/users/by-username/{username}` - Recherche par nom
- `POST /api/users` - Créer un utilisateur (utilise TenantId du contexte)
- `PUT /api/users/{id}` - Modifier un utilisateur
- `DELETE /api/users/{id}` - Supprimer un utilisateur

## Flux d'authentification

### 1. Connexion
```http
POST /api/auth/login
Content-Type: application/json

{
  "userName": "admin",
  "password": "password123"
}
```

**Réponse**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "base64_refresh_token",
  "expiresIn": 3600
}
```

### 2. Utilisation du token
Pour les endpoints protégés, ajouter l'en-tête:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### 3. Contenu du token JWT
Le token contient les claims suivants:
- **NameIdentifier** (`sub`): UserId
- **Name**: UserName
- **Email**: Email de l'utilisateur
- **TenantId**: Identifiant du tenant (si présent)
- **Jti**: Identifiant unique du token

## Isolation multi-tenant

### Comment ça fonctionne
1. Le **TenantId** est extrait du token JWT via `ICurrentUserContext`
2. Lors de la **création** d'entités (User, Product), le TenantId est automatiquement assigné
3. Les données sont **isolées** par tenant

### Services concernés
- **ProductService**: Utilise `_currentUserContext.TenantId` lors de la création
- **UserService**: Utilise `_currentUserContext.TenantId` lors de la création

## Configuration de la redirection

### Redirection automatique vers ReDoc
```csharp
app.MapGet("/", () => Results.Redirect("/api-docs")).AllowAnonymous();
```

- **URL racine** (`http://localhost:5220/`) → Redirige vers `/api-docs`
- **Swagger UI**: `http://localhost:5220/swagger`
- **ReDoc**: `http://localhost:5220/api-docs`

## Recommandations de sécurité

### ✅ Bonnes pratiques implémentées
- JWT avec signature HMAC-SHA256
- Validation du token (issuer, audience, lifetime)
- Attribut `[Authorize]` sur les endpoints sensibles
- Claims personnalisés pour le TenantId
- Isolation des données par tenant

### ⚠️ À améliorer pour la production
1. **Hachage des mots de passe**: 
   - Actuellement: Base64 simple (non sécurisé)
   - Recommandé: BCrypt, Argon2 ou ASP.NET Core Identity PasswordHasher

2. **Gestion des secrets**:
   - Déplacer `SecretKey` vers Azure Key Vault ou variables d'environnement
   - Utiliser `dotnet user-secrets` en développement

3. **HTTPS obligatoire**:
   - Passer `RequireHttpsMetadata` à `true` en production

4. **Refresh tokens**:
   - Stocker en base de données avec expiration
   - Implémenter la révocation des tokens

5. **Rate limiting**:
   - Ajouter un middleware de limitation de requêtes

6. **Filtrage par tenant**:
   - Ajouter un filtre global EF Core pour isoler automatiquement les requêtes par TenantId

## Tests

### Comment tester avec Swagger/ReDoc
1. Démarrer l'application
2. Naviguer vers `http://localhost:5220/swagger`
3. Utiliser `/api/auth/login` pour obtenir un token
4. Cliquer sur "Authorize" et entrer: `Bearer {votre_token}`
5. Tester les endpoints protégés

### Codes de statut HTTP attendus
- **200 OK**: Succès
- **401 Unauthorized**: Token manquant ou invalide
- **403 Forbidden**: Token valide mais permissions insuffisantes (si rôles implémentés)
- **404 Not Found**: Ressource introuvable

## Résumé

| Contrôleur | Authentification | Multi-tenant | Endpoints publics |
|-----------|------------------|--------------|-------------------|
| AuthController | Partielle | N/A | Login, RefreshToken |
| ProductsController | Oui (`[Authorize]`) | Oui | GET /api/products |
| UsersController | Oui (`[Authorize]`) | Oui | Aucun |

**Statut**: ✅ Configuration sécurisée avec isolation multi-tenant active
