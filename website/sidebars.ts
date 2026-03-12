import type {SidebarsConfig} from '@docusaurus/plugin-content-docs';

const sidebars: SidebarsConfig = {
  tutorialSidebar: [
    'intro',
    {
      type: 'category',
      label: 'Démarrage Rapide',
      items: ['getting-started/quick-start'],
    },
    {
      type: 'category',
      label: 'Architecture & Concepts',
      items: ['architecture/ddd'],
    },
    {
      type: 'category',
      label: 'Modules Intégrés',
      items: [
        'modules/identity',
        'modules/tenant-management',
      ],
    }
  ],
};

export default sidebars;
