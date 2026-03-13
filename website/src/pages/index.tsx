import type {ReactNode} from 'react';
import clsx from 'clsx';
import Link from '@docusaurus/Link';
import useDocusaurusContext from '@docusaurus/useDocusaurusContext';
import Layout from '@theme/Layout';
import HomepageFeatures from '@site/src/components/HomepageFeatures';
import Heading from '@theme/Heading';

import styles from './index.module.css';

function HomepageHeader() {
  const {siteConfig} = useDocusaurusContext();
  return (
    <header className={clsx('hero hero--primary', styles.heroBanner)}>
      <div className="container">
        <div className="badge badge--secondary margin-bottom--md">🚀 Version 2.1.0 Disponible</div>
        <Heading as="h1" className="hero__title">
          {siteConfig.title}
        </Heading>
        <p className="hero__subtitle" style={{maxWidth: '800px', margin: '0 auto 2rem auto'}}>
          Accélérez le développement de vos applications SaaS avec une Clean Architecture robuste, 
          le Multi-Tenancy natif et des outils de génération de code visuels.
        </p>
        <div className={styles.buttons}>
          <Link
            className="button button--secondary button--lg margin-right--md"
            to="/docs/intro">
            Découvrir la Doc 📖
          </Link>
          <Link
            className="button button--outline button--secondary button--lg"
            to="/docs/getting-started/first-steps">
            Tutoriel 10 min ⏱️
          </Link>
        </div>
        <div className="margin-top--lg">
          <code className="hero-code">dotnet tool install -g KBA.CLI</code>
        </div>
      </div>
    </header>
  );
}

export default function Home(): ReactNode {
  const {siteConfig} = useDocusaurusContext();
  return (
    <Layout
      title={`Bienvenue sur ${siteConfig.title}`}
      description="Framework d'entreprise .NET 8 basé sur Clean Architecture et DDD">
      <HomepageHeader />
      <main>
        <div className="container margin-top--xl margin-bottom--xl text--center">
          <div className="row">
            <div className="col col--8 col--offset-2">
              <Heading as="h2">Conçu pour la Production</Heading>
              <p className="lead">
                KBA Framework n'est pas juste un template. C'est un écosystème complet qui gère 
                les aspects complexes des applications modernes : sécurité, audit, jobs en arrière-plan, 
                et isolation des données.
              </p>
            </div>
          </div>
        </div>
        <HomepageFeatures />
        
        <div className="container margin-top--xl margin-bottom--xl">
          <div className="card shadow--md" style={{background: 'var(--ifm-color-emphasis-100)', border: 'none'}}>
            <div className="card__body padding--xl">
              <div className="row">
                <div className="col col--6">
                  <Heading as="h3">🛠️ KBA Studio & CLI</Heading>
                  <p>
                    Ne perdez plus de temps sur le code répétitif. Utilisez nos outils pour générer 
                    vos solutions, vos entités et vos migrations en un clic.
                  </p>
                  <ul>
                    <li>Génération Full-Stack (Domain, API, DTOs)</li>
                    <li>Support Microservices Cloud-Native</li>
                    <li>Gestion visuelle des migrations EF Core</li>
                  </ul>
                </div>
                <div className="col col--6 text--center">
                   <div style={{fontSize: '5rem'}}>💻</div>
                   <div className="margin-top--md">
                     <Link className="button button--primary" href="https://github.com/khalilbenaz/KBA.Framework">
                       Voir sur GitHub
                     </Link>
                   </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </main>
    </Layout>
  );
}
