import { defineConfig } from 'vitepress'

export default defineConfig({
  title: 'RIVORA Framework',
  description: 'Enterprise .NET 9 Framework - Clean Architecture, DDD, Multi-tenancy',

  base: '/RIVORA/',

  locales: {
    root: {
      label: 'Francais',
      lang: 'fr',
      themeConfig: {
        nav: [
          { text: 'Guide', link: '/guide/getting-started' },
          { text: 'Modules', link: '/modules/' },
          { text: 'CLI', link: '/cli/' },
          { text: 'Frontend', link: '/frontend/' },
          { text: 'Samples', link: '/samples/' },
          { text: 'API Reference', link: '/api/' },
          { text: 'Telecharger', link: '/download' }
        ],
        sidebar: {
          '/guide/': [
            { text: 'Introduction', items: [
              { text: 'Demarrage rapide', link: '/guide/getting-started' },
              { text: 'Installation', link: '/guide/installation' },
              { text: 'Creer son projet', link: '/guide/create-project' },
              { text: 'Architecture', link: '/guide/architecture' }
            ]},
            { text: 'Concepts', items: [
              { text: 'Clean Architecture', link: '/guide/clean-architecture' },
              { text: 'Multi-Tenancy', link: '/guide/multi-tenancy' },
              { text: 'Securite', link: '/guide/security' },
              { text: 'CQRS & MediatR', link: '/guide/cqrs' },
              { text: 'Value Objects', link: '/guide/value-objects' }
            ]},
            { text: 'Avance', items: [
              { text: 'GraphQL', link: '/guide/graphql' },
              { text: 'Webhooks', link: '/guide/webhooks' },
              { text: 'Export PDF/Excel', link: '/guide/export' },
              { text: 'AI & RAG', link: '/guide/ai-rag' },
              { text: 'NL Query', link: '/guide/natural-query' },
              { text: 'Event Sourcing', link: '/guide/event-sourcing' }
            ]},
            { text: 'Integration', items: [
              { text: 'OAuth2 / OIDC', link: '/guide/oauth' },
              { text: 'Conformite RGPD', link: '/guide/gdpr' },
              { text: 'Facturation SaaS', link: '/guide/billing' },
              { text: 'Docker', link: '/guide/docker' }
            ]},
            { text: 'DevOps', items: [
              { text: 'Docker', link: '/guide/docker' },
              { text: 'CI/CD', link: '/guide/ci-cd' },
              { text: 'Native AOT', link: '/guide/native-aot' },
              { text: 'Monitoring (Grafana)', link: '/guide/monitoring' }
            ]}
          ],
          '/modules/': [
            { text: 'Infrastructure', items: [
              { text: 'Core', link: '/modules/core' },
              { text: 'Security', link: '/modules/security' },
              { text: 'Caching', link: '/modules/caching' },
              { text: 'Jobs', link: '/modules/jobs' }
            ]},
            { text: 'Fonctionnalites', items: [
              { text: 'Export', link: '/modules/export' },
              { text: 'Webhooks', link: '/modules/webhooks' },
              { text: 'GraphQL', link: '/modules/graphql' },
              { text: 'Billing', link: '/modules/billing' },
              { text: 'SMS', link: '/modules/sms' },
              { text: 'Localisation', link: '/modules/localization' },
              { text: 'Audit Logging UI', link: '/modules/audit' },
              { text: 'Plugin System', link: '/modules/plugins' }
            ]},
            { text: 'Domaine', items: [
              { text: 'Event Sourcing', link: '/modules/eventsourcing' },
              { text: 'Saga / Process Manager', link: '/modules/saga' },
              { text: 'Privacy RGPD', link: '/modules/privacy' }
            ]},
            { text: 'Identite & Tenancy', items: [
              { text: 'Identity.Pro', link: '/modules/identity' },
              { text: 'Multi-tenancy & SaaS', link: '/modules/multitenancy' }
            ]},
            { text: 'AI & Integration', items: [
              { text: 'AI & NaturalQuery', link: '/modules/ai' },
              { text: 'AI Guardrails', link: '/modules/guardrails' },
              { text: 'AI Agents', link: '/modules/agents' },
              { text: 'RivoraApiClient', link: '/modules/client' }
            ]}
          ],
          '/cli/': [
            { text: 'CLI', items: [
              { text: 'Vue d\'ensemble', link: '/cli/' },
            ]},
            { text: 'Scaffolding', items: [
              { text: 'rvr new', link: '/cli/new' },
              { text: 'rvr generate', link: '/cli/generate' },
              { text: 'rvr add-module', link: '/cli/add-module' },
              { text: 'rvr remove-module', link: '/cli/remove-module' },
            ]},
            { text: 'AI', items: [
              { text: 'rvr ai', link: '/cli/ai' },
            ]},
            { text: 'Base de donnees', items: [
              { text: 'rvr migrate', link: '/cli/migrate' },
            ]},
            { text: 'Environnements', items: [
              { text: 'rvr env', link: '/cli/env' },
            ]},
            { text: 'Publication & DevOps', items: [
              { text: 'rvr publish', link: '/cli/publish' },
              { text: 'rvr upgrade', link: '/cli/upgrade' },
              { text: 'rvr doctor', link: '/cli/doctor' },
              { text: 'rvr benchmark', link: '/cli/benchmark' },
            ]},
          ],
          '/api/': [
            { text: 'API Reference', items: [
              { text: 'Vue d\'ensemble', link: '/api/' },
              { text: 'Authentication', link: '/api/auth' },
              { text: 'Users', link: '/api/users' },
              { text: 'Products', link: '/api/products' },
              { text: 'Tenants', link: '/api/tenants' },
              { text: 'Webhooks', link: '/api/webhooks' },
              { text: 'Health Checks', link: '/api/health' },
              { text: 'Initialization', link: '/api/init' },
            ]},
          ],
          '/frontend/': [
            { text: 'React Frontend', items: [
              { text: 'Introduction', link: '/frontend/' },
              { text: 'Installation', link: '/frontend/installation' },
              { text: 'Architecture', link: '/frontend/architecture' },
              { text: 'Routing', link: '/frontend/routing' },
              { text: 'Authentification', link: '/frontend/authentication' },
              { text: 'Etat global (Zustand)', link: '/frontend/state-management' },
              { text: 'i18n (FR/EN)', link: '/frontend/i18n' },
              { text: 'Dark Mode', link: '/frontend/dark-mode' },
            ]},
            { text: 'Pages', items: [
              { text: 'Landing Page', link: '/frontend/pages/landing' },
              { text: 'Dashboard', link: '/frontend/pages/dashboard' },
              { text: 'Chat', link: '/frontend/pages/chat' },
              { text: 'Flow Builder', link: '/frontend/pages/flow-builder' },
              { text: 'Project Wizard', link: '/frontend/pages/project-wizard' },
              { text: 'Entity Generator', link: '/frontend/pages/entity-generator' },
              { text: 'Kanban', link: '/frontend/pages/kanban' },
              { text: 'Analytics', link: '/frontend/pages/analytics' },
              { text: 'Webhooks', link: '/frontend/pages/webhooks' },
            ]},
            { text: 'Composants', items: [
              { text: 'Badge', link: '/frontend/components/badge' },
              { text: 'StatCard', link: '/frontend/components/stat-card' },
              { text: 'Pagination', link: '/frontend/components/pagination' },
              { text: 'TableSkeleton', link: '/frontend/components/table-skeleton' },
              { text: 'NotificationBell', link: '/frontend/components/notification-bell' },
              { text: 'Charts (SVG)', link: '/frontend/components/charts' },
            ]},
            { text: 'Tests', items: [
              { text: 'Unit Tests (Vitest)', link: '/frontend/testing/unit' },
              { text: 'E2E Tests (Playwright)', link: '/frontend/testing/e2e' },
            ]}
          ],
          '/samples/': [
            { text: 'Exemples', items: [
              { text: 'Vue d\'ensemble', link: '/samples/' },
              { text: 'SaaS Starter', link: '/samples/saas-starter' },
              { text: 'E-commerce', link: '/samples/ecommerce' },
              { text: 'AI RAG App', link: '/samples/ai-rag' },
              { text: 'Fintech Payment', link: '/samples/fintech' },
            ]}
          ]
        }
      }
    },
    en: {
      label: 'English',
      lang: 'en',
      themeConfig: {
        nav: [
          { text: 'Guide', link: '/en/guide/getting-started' },
          { text: 'Modules', link: '/en/modules/' },
          { text: 'CLI', link: '/en/cli/' },
          { text: 'API Reference', link: '/api/' },
          { text: 'Download', link: '/en/download' }
        ],
        sidebar: {
          '/en/guide/': [
            { text: 'Introduction', items: [
              { text: 'Quick Start', link: '/en/guide/getting-started' },
              { text: 'Installation', link: '/en/guide/installation' },
              { text: 'Create a Project', link: '/guide/create-project' },
              { text: 'Architecture', link: '/en/guide/architecture' }
            ]},
            { text: 'Concepts', items: [
              { text: 'Clean Architecture', link: '/en/guide/clean-architecture' },
              { text: 'Multi-Tenancy', link: '/en/guide/multi-tenancy' },
              { text: 'Security', link: '/en/guide/security' },
              { text: 'CQRS & MediatR', link: '/en/guide/cqrs' },
              { text: 'Value Objects', link: '/guide/value-objects' }
            ]},
            { text: 'Advanced', items: [
              { text: 'GraphQL', link: '/en/guide/graphql' },
              { text: 'Webhooks', link: '/en/guide/webhooks' },
              { text: 'Export PDF/Excel', link: '/en/guide/export' },
              { text: 'AI & RAG', link: '/en/guide/ai-rag' },
              { text: 'NL Query', link: '/en/guide/natural-query' },
              { text: 'Event Sourcing', link: '/guide/event-sourcing' }
            ]},
            { text: 'Integration', items: [
              { text: 'OAuth2 / OIDC', link: '/guide/oauth' },
              { text: 'GDPR Compliance', link: '/guide/gdpr' },
              { text: 'SaaS Billing', link: '/guide/billing' },
              { text: 'Docker', link: '/guide/docker' }
            ]}
          ],
          '/en/modules/': [
            { text: 'Infrastructure', items: [
              { text: 'Core', link: '/en/modules/core' },
              { text: 'Security', link: '/en/modules/security' },
              { text: 'Caching', link: '/en/modules/caching' },
              { text: 'Jobs', link: '/en/modules/jobs' }
            ]},
            { text: 'Features', items: [
              { text: 'Export', link: '/en/modules/export' },
              { text: 'Webhooks', link: '/en/modules/webhooks' },
              { text: 'GraphQL', link: '/en/modules/graphql' },
              { text: 'Billing', link: '/en/modules/billing' },
              { text: 'Localization', link: '/modules/localization' },
              { text: 'Audit Logging UI', link: '/modules/audit' }
            ]},
            { text: 'Domain', items: [
              { text: 'Event Sourcing', link: '/modules/eventsourcing' },
              { text: 'Saga / Process Manager', link: '/modules/saga' },
              { text: 'GDPR Privacy', link: '/modules/privacy' }
            ]},
            { text: 'Identity & Tenancy', items: [
              { text: 'Identity.Pro', link: '/modules/identity' },
              { text: 'Multi-tenancy & SaaS', link: '/modules/multitenancy' }
            ]},
            { text: 'AI & Integration', items: [
              { text: 'AI & NaturalQuery', link: '/modules/ai' },
              { text: 'RivoraApiClient', link: '/modules/client' }
            ]}
          ]
        }
      }
    }
  },

  themeConfig: {
    search: { provider: 'local' },
    socialLinks: [
      { icon: 'github', link: 'https://github.com/khalilbenaz/RIVORA' }
    ],
    footer: {
      message: 'Released under the MIT License.',
      copyright: 'Copyright 2024-2026 Khalil Benazzouz'
    }
  }
})
