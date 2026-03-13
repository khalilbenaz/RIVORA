import {themes as prismThemes} from 'prism-react-renderer';
import type {Config} from '@docusaurus/types';
import type * as Preset from '@docusaurus/preset-classic';

// This runs in Node.js - Don't use client-side code here (browser APIs, JSX...)

const config: Config = {
  title: 'KBA Framework',
  tagline: 'Modern Clean Architecture for .NET 8',
  favicon: 'img/favicon.ico',

  // Future flags, see https://docusaurus.io/docs/api/docusaurus-config#future
  future: {
    v4: true, // Improve compatibility with the upcoming Docusaurus v4
  },

  // Set the production url of your site here
  url: 'https://khalilbenaz.github.io',
  // For GitHub pages deployment, it is often '/<projectName>/'
  baseUrl: '/KBA.Framework/',

  // GitHub pages deployment config.
  organizationName: 'khalilbenaz',
  projectName: 'KBA.Framework',
  deploymentBranch: 'gh-pages',
  trailingSlash: false,

  onBrokenLinks: 'warn',
  onBrokenMarkdownLinks: 'warn',

  i18n: {
    defaultLocale: 'fr',
    locales: ['fr'],
  },

  presets: [
    [
      'classic',
      {
        docs: {
          sidebarPath: './sidebars.ts',
          editUrl: 'https://github.com/khalilbenaz/KBA.Framework/tree/main/website/',
        },
        theme: {
          customCss: './src/css/custom.css',
        },
      } satisfies Preset.Options,
    ],
  ],

  themeConfig: {
    metadata: [
      {name: 'keywords', content: 'dotnet, clean architecture, ddd, multitenancy, framework, saas'},
      {name: 'twitter:card', content: 'summary_large_image'},
    ],
    navbar: {
      title: 'KBA Framework',
      hideOnScroll: true,
      logo: {
        alt: 'KBA Logo',
        src: 'img/logo.svg',
        srcDark: 'img/logo.svg',
      },
      items: [
        {
          type: 'docSidebar',
          sidebarId: 'tutorialSidebar',
          position: 'left',
          label: 'Documentation',
        },
        {to: '/docs/getting-started/first-steps', label: 'Tutoriel', position: 'left'},
        {
          href: 'https://github.com/khalilbenaz/KBA.Framework',
          label: 'GitHub',
          position: 'right',
        },
      ],
    },
    footer: {
      style: 'dark',
      links: [
        {
          title: 'Apprendre',
          items: [
            {label: 'Introduction', to: '/docs/intro'},
            {label: 'Quick Start', to: '/docs/getting-started/quick-start'},
            {label: 'Pas-à-Pas', to: '/docs/getting-started/first-steps'},
          ],
        },
        {
          title: 'Architecture',
          items: [
            {label: 'Clean Architecture', to: '/docs/intro'},
            {label: 'Multi-Tenancy', to: '/docs/intro'},
          ],
        },
        {
          title: 'Plus',
          items: [
            {label: 'GitHub', href: 'https://github.com/khalilbenaz/KBA.Framework'},
          ],
        },
      ],
      copyright: `Copyright © ${new Date().getFullYear()} KBA Framework. Built with Docusaurus.`,
    },
    prism: {
      theme: prismThemes.github,
      darkTheme: prismThemes.dracula,
      additionalLanguages: ['csharp', 'powershell', 'bash', 'json', 'docker'],
    },
  } satisfies Preset.ThemeConfig,
};

export default config;
