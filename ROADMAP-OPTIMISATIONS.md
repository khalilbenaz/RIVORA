# KBA.Framework - Roadmap d'Optimisations & Nouvelles Features

> Document generated on 2026-03-13 | Framework v2.1.0 (.NET 8)

---

## Table des matières

1. [Optimisations de Performance](#1-optimisations-de-performance)
2. [Améliorations d'Architecture](#2-améliorations-darchitecture)
3. [Nouvelles Features](#3-nouvelles-features)
4. [Sécurité & Conformité](#4-sécurité--conformité)
5. [Developer Experience (DX)](#5-developer-experience-dx)
6. [Observabilité & Monitoring](#6-observabilité--monitoring)
7. [Migration .NET 9 / 10](#7-migration-net-9--10)
8. [Écosystème & Intégrations](#8-écosystème--intégrations)

---

## 1. Optimisations de Performance

### 1.1 Compiled Queries EF Core
- **Quoi** : Remplacer les requêtes LINQ dynamiques par des `EF.CompileAsyncQuery` dans les repositories les plus sollicités (GetById, GetAll avec pagination)
- **Impact** : Élimination du coût de compilation LINQ à chaque appel (~15-40% de gain sur les hot paths)
- **Priorité** : Haute

### 1.2 Bulk Operations
- **Quoi** : Intégrer `EFCore.BulkExtensions` ou `linq2db.EntityFrameworkCore` pour les opérations de masse (BulkInsert, BulkUpdate, BulkDelete)
- **Impact** : Réduction drastique du temps d'insertion/mise à jour (x10 à x100 sur les gros volumes)
- **Priorité** : Haute

### 1.3 Response Compression
- **Quoi** : Ajouter le middleware `ResponseCompression` avec Brotli + Gzip dans la couche Api
- **Impact** : Réduction de 60-80% de la taille des payloads JSON
- **Priorité** : Moyenne

### 1.4 Output Caching (.NET 8+)
- **Quoi** : Remplacer/compléter le Response Caching existant par le nouveau `OutputCaching` de .NET 8, avec support des tags pour invalidation granulaire
- **Impact** : Caching natif plus performant, invalidation par tag intégrée (similaire au tag-based cache existant dans `KBA.Framework.Caching`)
- **Priorité** : Moyenne

### 1.5 Connection Pooling & Resiliency améliorés
- **Quoi** : Configurer `DbContextPooling` (`AddDbContextPool`) au lieu de `AddDbContext` + tuning des pools de connexions par provider
- **Impact** : Réduction de l'overhead de création de DbContext (~30% de gain en throughput)
- **Priorité** : Haute

### 1.6 Projection automatique (Select)
- **Quoi** : Remplacer les `AutoMapper.Map<T>()` post-query par `ProjectTo<T>()` directement dans les queries EF Core
- **Impact** : Réduction des colonnes sélectionnées en SQL, moins de données transférées
- **Priorité** : Moyenne

### 1.7 Cache distribué L2 avec HybridCache
- **Quoi** : Utiliser le nouveau `HybridCache` (.NET 9+) qui combine L1 (mémoire) et L2 (Redis) avec stampede protection native
- **Impact** : Simplification de l'architecture de cache, protection contre le cache stampede
- **Priorité** : Basse (dépend de la migration .NET 9)

### 1.8 Pagination par Cursor (Keyset Pagination)
- **Quoi** : Ajouter une option de pagination par curseur en alternative à OFFSET/LIMIT dans les repositories
- **Impact** : Performances constantes quelle que soit la page (vs dégradation linéaire avec OFFSET)
- **Priorité** : Moyenne

---

## 2. Améliorations d'Architecture

### 2.1 Outbox Pattern pour les Domain Events
- **Quoi** : Implémenter le Transactional Outbox Pattern pour garantir la publication fiable des domain events
- **Impact** : Consistance garantie entre l'écriture en base et la publication d'événements (at-least-once delivery)
- **Priorité** : Haute

### 2.2 Specification Pattern formalisé
- **Quoi** : Créer un module `KBA.Framework.Specifications` avec `ISpecification<T>`, combinaison de spécifications (And, Or, Not), et intégration dans les repositories
- **Impact** : Encapsulation des règles de requêtage complexes, réutilisabilité, testabilité
- **Priorité** : Moyenne

### 2.3 Result Pattern (Railway-Oriented)
- **Quoi** : Introduire un type `Result<T>` / `Result<T, TError>` pour remplacer les exceptions comme flow de contrôle dans la couche Application
- **Impact** : Code plus explicite, moins d'exceptions pour les cas métier prévisibles, meilleure composition des opérations
- **Priorité** : Haute

### 2.4 Module Boundaries & Modular Monolith
- **Quoi** : Ajouter un support de modules isolés (Modular Monolith pattern) avec communication inter-modules via MediatR ou un bus interne
- **Impact** : Préparation à l'extraction en microservices, isolation des domaines métier
- **Priorité** : Moyenne

### 2.5 Event Sourcing (optionnel)
- **Quoi** : Module optionnel `KBA.Framework.EventSourcing` avec support EventStoreDB ou Marten
- **Impact** : Historique complet des changements d'état, audit natif, replay d'événements
- **Priorité** : Basse

### 2.6 Saga / Process Manager
- **Quoi** : Implémenter un pattern Saga pour orchestrer les transactions distribuées multi-étapes
- **Impact** : Gestion cohérente des workflows métier complexes avec compensation automatique
- **Priorité** : Basse

---

## 3. Nouvelles Features

### 3.1 Real-Time avec SignalR
- **Quoi** : Module `KBA.Framework.RealTime` avec SignalR Hubs, support de groupes par tenant, notifications push
- **Impact** : Communication temps réel (notifications, dashboards live, collaborative features)
- **Priorité** : Haute

### 3.2 File Storage Abstraction
- **Quoi** : Module `KBA.Framework.Storage` avec interface `IFileStorage` et implémentations pour Azure Blob, AWS S3, MinIO, et filesystem local
- **Impact** : Upload/download de fichiers avec gestion des métadonnées, thumbnails, virus scanning
- **Priorité** : Haute

### 3.3 Email & Notifications
- **Quoi** : Module `KBA.Framework.Notifications` avec support multi-canal (Email via SMTP/SendGrid/SES, SMS via Twilio, Push via Firebase)
- **Impact** : Système de notifications unifié avec templates, queuing, et retry
- **Priorité** : Haute

### 3.4 Workflow Engine
- **Quoi** : Module `KBA.Framework.Workflows` basé sur Elsa Workflows ou un moteur custom léger
- **Impact** : Workflows métier visuels, approbations, state machines configurables
- **Priorité** : Moyenne

### 3.5 GraphQL Support
- **Quoi** : Module `KBA.Framework.GraphQL` avec Hot Chocolate, auto-génération du schema depuis les entités
- **Impact** : Alternative à REST pour les clients ayant besoin de requêtes flexibles
- **Priorité** : Moyenne

### 3.6 gRPC Support
- **Quoi** : Module `KBA.Framework.Grpc` avec génération de proto files depuis les DTOs, support de streaming
- **Impact** : Communication inter-services haute performance pour les architectures microservices
- **Priorité** : Moyenne

### 3.7 Localization & i18n
- **Quoi** : Module `KBA.Framework.Localization` avec support de ressources, base de données, ou JSON, middleware de culture auto-detect
- **Impact** : Applications multi-langues out-of-the-box
- **Priorité** : Moyenne

### 3.8 Data Import/Export
- **Quoi** : Module `KBA.Framework.DataExchange` avec import/export CSV, Excel (EPPlus/ClosedXML), JSON, et PDF (QuestPDF)
- **Impact** : Génération de rapports, imports de données en masse, exports personnalisés
- **Priorité** : Moyenne

### 3.9 Search Engine Integration
- **Quoi** : Module `KBA.Framework.Search` avec abstraction pour Elasticsearch/OpenSearch et Azure Cognitive Search
- **Impact** : Recherche full-text performante, faceting, suggestions, highlighting
- **Priorité** : Basse

### 3.10 Webhooks
- **Quoi** : Module `KBA.Framework.Webhooks` pour enregistrer, gérer et déclencher des webhooks sortants avec retry et signature HMAC
- **Impact** : Intégration événementielle avec des systèmes tiers
- **Priorité** : Moyenne

### 3.11 Multi-Database Read Replicas
- **Quoi** : Support du read/write splitting avec routage automatique des queries vers les replicas en lecture
- **Impact** : Scalabilité horizontale pour les charges en lecture intensive
- **Priorité** : Basse

### 3.12 Soft Delete amélioré
- **Quoi** : Global query filters pour le soft delete avec support de cascade, restore, purge planifiée, et audit
- **Impact** : Suppression logique cohérente dans tout le framework avec gestion du cycle de vie
- **Priorité** : Moyenne

---

## 4. Sécurité & Conformité

### 4.1 OAuth 2.0 / OpenID Connect
- **Quoi** : Intégration avec des identity providers externes (Azure AD, Keycloak, Auth0, Google, GitHub) via OpenIddict ou IdentityServer
- **Impact** : SSO, social login, federation d'identité
- **Priorité** : Haute

### 4.2 GDPR Compliance Toolkit
- **Quoi** : Module `KBA.Framework.Privacy` avec data anonymization, right-to-erasure, consent management, data portability (export)
- **Impact** : Conformité RGPD/GDPR simplifiée pour toutes les applications
- **Priorité** : Haute

### 4.3 API Key Management
- **Quoi** : Gestion d'API keys avec scopes, expiration, rotation automatique, et throttling par key
- **Impact** : Authentification M2M (machine-to-machine) et intégrations tierces
- **Priorité** : Moyenne

### 4.4 Data Encryption at Rest
- **Quoi** : Chiffrement transparent des colonnes sensibles en base (EF Core Value Converters + Azure Key Vault / AWS KMS)
- **Impact** : Protection des données PII/PHI même en cas de compromission de la base
- **Priorité** : Haute

### 4.5 Security Headers Middleware
- **Quoi** : Middleware configurant automatiquement CSP, HSTS, X-Frame-Options, X-Content-Type-Options, Referrer-Policy
- **Impact** : Protection OWASP par défaut sans configuration manuelle
- **Priorité** : Moyenne

### 4.6 IP Whitelisting & Geo-Blocking
- **Quoi** : Middleware de filtrage par IP/plage/pays avec intégration GeoIP (MaxMind)
- **Impact** : Restriction d'accès géographique, conformité réglementaire
- **Priorité** : Basse

### 4.7 Secret Rotation automatique
- **Quoi** : Rotation automatique des JWT signing keys, connection strings, et API keys avec zero-downtime
- **Impact** : Réduction du risque en cas de fuite de secrets
- **Priorité** : Moyenne

---

## 5. Developer Experience (DX)

### 5.1 KBA Studio - Visual Query Builder
- **Quoi** : Étendre KBA Studio avec un query builder visuel pour construire des spécifications et des filtres complexes via drag & drop
- **Impact** : Accélération du développement, moins d'erreurs dans les requêtes
- **Priorité** : Moyenne

### 5.2 KBA CLI - Migration Generator
- **Quoi** : Commande `kba migrate generate` qui analyse les changements de modèle et génère les migrations EF Core automatiquement
- **Impact** : Workflow de migration simplifié
- **Priorité** : Moyenne

### 5.3 KBA CLI - API Client Generator
- **Quoi** : Commande `kba generate client` qui génère un SDK client TypeScript/C# à partir du schema OpenAPI
- **Impact** : Consommation type-safe des APIs depuis le frontend ou d'autres services
- **Priorité** : Haute

### 5.4 DevContainer & Codespaces
- **Quoi** : Configuration `.devcontainer/` avec Docker Compose (SQL Server, Redis, RabbitMQ) pour un onboarding en un clic
- **Impact** : Environnement de développement reproductible et instant
- **Priorité** : Moyenne

### 5.5 Hot Reload amélioré
- **Quoi** : Support de hot reload pour les templates Scriban du CLI et les configurations de feature flags
- **Impact** : Boucle de feedback plus rapide en développement
- **Priorité** : Basse

### 5.6 Scaffolding de Tests
- **Quoi** : Commande `kba generate test <entity>` qui génère les tests unitaires et d'intégration correspondants à une entité/service
- **Impact** : Couverture de tests accélérée, conventions de test uniformes
- **Priorité** : Moyenne

### 5.7 Documentation interactive (OpenAPI)
- **Quoi** : Remplacer Swagger UI par Scalar ou RapiDoc pour une documentation API plus moderne et interactive
- **Impact** : Meilleure DX pour les consommateurs d'API
- **Priorité** : Basse

### 5.8 NuGet Packaging
- **Quoi** : Publier chaque module comme un package NuGet indépendant sur nuget.org
- **Impact** : Adoption modulaire du framework, versioning indépendant par module
- **Priorité** : Haute

---

## 6. Observabilité & Monitoring

### 6.1 Distributed Tracing complet
- **Quoi** : Enrichir l'intégration OpenTelemetry existante avec des spans custom pour les domain events, les jobs background, et les opérations de cache
- **Impact** : Traçabilité complète des requêtes à travers tous les composants
- **Priorité** : Haute

### 6.2 Metrics custom
- **Quoi** : Exposer des métriques métier via `System.Diagnostics.Metrics` (nombre d'utilisateurs actifs, requêtes par tenant, cache hit ratio, job throughput)
- **Impact** : Dashboards métier dans Grafana/Prometheus
- **Priorité** : Moyenne

### 6.3 Structured Logging enrichi
- **Quoi** : Enrichisseurs Serilog automatiques pour TenantId, UserId, CorrelationId, RequestPath sur tous les logs
- **Impact** : Debugging cross-tenant simplifié, corrélation des logs
- **Priorité** : Haute

### 6.4 Alerting Framework
- **Quoi** : Module `KBA.Framework.Alerting` avec règles d'alerte configurables, canaux de notification (Slack, Teams, PagerDuty, email)
- **Impact** : Réaction proactive aux anomalies
- **Priorité** : Basse

### 6.5 Audit Dashboard
- **Quoi** : UI web pour visualiser les audit logs existants avec filtres, timeline, et export
- **Impact** : Visibilité sur les changements et actions utilisateur
- **Priorité** : Moyenne

### 6.6 Performance Profiling intégré
- **Quoi** : Intégration de MiniProfiler pour le profiling en développement (requêtes SQL, temps de réponse, cache hits)
- **Impact** : Identification rapide des bottlenecks en dev
- **Priorité** : Moyenne

---

## 7. Migration .NET 9 / 10

### 7.1 .NET 9 LTS Migration
- **Quoi** : Migrer le framework vers .NET 9 pour bénéficier des améliorations de performance et des nouvelles APIs
- **Impact** : Gains de performance natifs (LINQ, EF Core 9, JSON serialization), HybridCache, `System.Threading.Lock`
- **Priorité** : Haute

### 7.2 Native AOT Support
- **Quoi** : Rendre les modules core compatibles Native AOT (élimination de la reflection, source generators)
- **Impact** : Startup time < 100ms, empreinte mémoire réduite (idéal pour serverless/containers)
- **Priorité** : Moyenne

### 7.3 Source Generators
- **Quoi** : Remplacer AutoMapper par des source generators (Mapperly) et générer les validators/handlers au compile-time
- **Impact** : Zéro overhead de reflection, erreurs de mapping au compile-time
- **Priorité** : Haute

### 7.4 Minimal API Support
- **Quoi** : Ajouter un module `KBA.Framework.MinimalApi` comme alternative aux controllers MVC
- **Impact** : APIs plus légères et performantes, meilleure compatibilité AOT
- **Priorité** : Moyenne

### 7.5 Aspire Integration
- **Quoi** : Templates `kba new` compatibles .NET Aspire pour l'orchestration locale et le déploiement cloud-native
- **Impact** : Orchestration simplifiée des dépendances (Redis, SQL, RabbitMQ), dashboard intégré
- **Priorité** : Haute

---

## 8. Écosystème & Intégrations

### 8.1 Terraform / Pulumi Modules
- **Quoi** : Modules IaC pour déployer l'infrastructure nécessaire (Azure/AWS) en un seul `terraform apply`
- **Impact** : Infrastructure as Code pour un déploiement reproductible
- **Priorité** : Moyenne

### 8.2 Dapr Integration
- **Quoi** : Module `KBA.Framework.Dapr` pour le service invocation, pub/sub, state management, et secrets via Dapr
- **Impact** : Portabilité cloud-agnostic, simplification du code d'infrastructure
- **Priorité** : Basse

### 8.3 Message Broker Abstraction
- **Quoi** : Étendre l'intégration MassTransit existante avec des consumers auto-générés depuis les domain events et support de Kafka
- **Impact** : Event-driven architecture simplifiée
- **Priorité** : Moyenne

### 8.4 Admin Panel auto-généré
- **Quoi** : Module `KBA.Framework.Admin` qui génère automatiquement un panel d'administration CRUD basé sur les entités (Blazor ou React)
- **Impact** : Backoffice fonctionnel sans code supplémentaire
- **Priorité** : Haute

### 8.5 SDK Mobile (MAUI)
- **Quoi** : Package client .NET MAUI avec auth, sync offline, et push notifications pré-configurés
- **Impact** : Développement mobile rapide sur la base du framework
- **Priorité** : Basse

### 8.6 Plugin System
- **Quoi** : Système de plugins dynamiques permettant de charger/décharger des modules à runtime
- **Impact** : Extensibilité maximale, marketplace de plugins communautaires
- **Priorité** : Basse

---

## Matrice de Priorisation

| Feature | Impact | Effort | Priorité |
|---------|--------|--------|----------|
| Compiled Queries EF Core | Haut | Faible | P0 |
| Bulk Operations | Haut | Faible | P0 |
| DbContext Pooling | Haut | Faible | P0 |
| Result Pattern | Haut | Moyen | P0 |
| Outbox Pattern | Haut | Moyen | P0 |
| SignalR Real-Time | Haut | Moyen | P0 |
| File Storage Abstraction | Haut | Moyen | P0 |
| OAuth 2.0 / OIDC | Haut | Élevé | P0 |
| NuGet Packaging | Haut | Moyen | P0 |
| Source Generators (Mapperly) | Haut | Moyen | P1 |
| .NET 9 Migration | Haut | Élevé | P1 |
| Aspire Integration | Haut | Moyen | P1 |
| Notifications multi-canal | Haut | Moyen | P1 |
| GDPR Compliance | Haut | Élevé | P1 |
| Data Encryption at Rest | Haut | Moyen | P1 |
| Distributed Tracing enrichi | Moyen | Faible | P1 |
| Structured Logging enrichi | Moyen | Faible | P1 |
| API Client Generator | Moyen | Moyen | P1 |
| Admin Panel auto-généré | Haut | Élevé | P1 |
| Keyset Pagination | Moyen | Faible | P2 |
| Specification Pattern | Moyen | Moyen | P2 |
| GraphQL Support | Moyen | Élevé | P2 |
| gRPC Support | Moyen | Élevé | P2 |
| Workflow Engine | Moyen | Élevé | P2 |
| Webhooks | Moyen | Moyen | P2 |
| Event Sourcing | Bas | Élevé | P3 |
| Plugin System | Bas | Élevé | P3 |
| Dapr Integration | Bas | Élevé | P3 |

---

> **Légende** : P0 = Quick wins & fondations critiques | P1 = Prochaine itération | P2 = Moyen terme | P3 = Vision long terme
