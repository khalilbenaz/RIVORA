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
          { text: 'API Reference', link: '/api/' }
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
          { text: 'API Reference', link: '/api/' }
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
