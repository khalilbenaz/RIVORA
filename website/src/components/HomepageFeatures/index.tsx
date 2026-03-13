import type {ReactNode} from 'react';
import clsx from 'clsx';
import Heading from '@theme/Heading';
import styles from './styles.module.css';

type FeatureItem = {
  title: string;
  Svg: React.ComponentType<React.ComponentProps<'svg'>>;
  description: ReactNode;
};

const FeatureList: FeatureItem[] = [
  {
    title: 'Clean Architecture',
    Svg: require('@site/static/img/undraw_docusaurus_mountain.svg').default,
    description: (
      <>
        Structure robuste basée sur les principes de Clean Architecture et DDD, 
        séparant strictement le Domaine, l&apos;Application et l&apos;Infrastructure.
      </>
    ),
  },
  {
    title: 'Multi-Tenancy Natif',
    Svg: require('@site/static/img/undraw_docusaurus_tree.svg').default,
    description: (
      <>
        Isolation complète des données par tenant, supportant plusieurs stratégies 
        (row-level, schema, database) pour vos applications SaaS.
      </>
    ),
  },
  {
    title: 'Enterprise Features (Gratuit)',
    Svg: require('@site/static/img/undraw_docusaurus_react.svg').default,
    description: (
      <>
        Générez votre code via <strong>KBA Studio</strong> et profitez de modules premium inclus : SaaS Billing (Stripe), Impersonation, Audit Logs UI et Blob Storage.
      </>
    ),
  },
];

function Feature({title, Svg, description}: FeatureItem) {
  return (
    <div className={clsx('col col--4')}>
      <div className="text--center">
        <Svg className={styles.featureSvg} role="img" />
      </div>
      <div className="text--center padding-horiz--md">
        <Heading as="h3">{title}</Heading>
        <p>{description}</p>
      </div>
    </div>
  );
}

export default function HomepageFeatures(): ReactNode {
  return (
    <section className={styles.features}>
      <div className="container">
        <div className="row">
          {FeatureList.map((props, idx) => (
            <Feature key={idx} {...props} />
          ))}
        </div>
      </div>
    </section>
  );
}
