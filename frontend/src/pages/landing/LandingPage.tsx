import { Link } from 'react-router-dom';
import {
  Shield, Zap, Building2, Database, BarChart3, Lock,
  ChevronRight, Star, ArrowRight, Code2, Layers, Globe,
} from 'lucide-react';

const features = [
  { icon: Shield, title: 'Sécurité avancée', desc: 'JWT, RBAC, API Keys hashées, TOTP 2FA, rate limiting et headers OWASP intégrés.' },
  { icon: Building2, title: 'Multi-tenancy', desc: 'Isolation par tenant avec filtres automatiques, connection strings dédiées et onboarding SaaS.' },
  { icon: Database, title: 'Multi-database', desc: 'SQL Server, PostgreSQL, MySQL, SQLite. Changez de provider sans toucher au code métier.' },
  { icon: Zap, title: 'Performance', desc: 'Output caching, response compression, query splitting, AOT-ready et gRPC support.' },
  { icon: BarChart3, title: 'Observabilité', desc: 'OpenTelemetry, Serilog, Prometheus, Grafana dashboards et health checks intégrés.' },
  { icon: Lock, title: 'GDPR & Privacy', desc: 'Chiffrement at-rest AES-256, anonymisation, export de données et gestion du consentement.' },
];

const pricing = [
  { name: 'Starter', price: 'Gratuit', period: 'open source', features: ['Clean Architecture', 'Auth JWT', '1 tenant', 'Community support'], cta: 'Commencer', highlighted: false },
  { name: 'Pro', price: '49€', period: '/mois', features: ['Tout Starter +', 'Multi-tenancy', 'Plugins système', 'Email + SMS', 'Support prioritaire'], cta: 'Essai gratuit', highlighted: true },
  { name: 'Enterprise', price: 'Sur mesure', period: '', features: ['Tout Pro +', 'SSO (SAML/OIDC)', 'SLA 99.9%', 'Audit avancé', 'Support dédié'], cta: 'Nous contacter', highlighted: false },
];

const testimonials = [
  { name: 'Marie L.', role: 'CTO, FinTech', text: 'RIVORA nous a fait gagner 3 mois de développement. La clean architecture est impeccable.', rating: 5 },
  { name: 'Karim B.', role: 'Lead Dev, SaaS', text: 'Le multi-tenancy fonctionne out-of-the-box. On a pu se concentrer sur notre métier.', rating: 5 },
  { name: 'Sophie D.', role: 'Freelance', text: 'Le CLI est un game-changer. Générer un projet complet en une commande, c\'est magique.', rating: 5 },
];

export default function LandingPage() {
  return (
    <div className="min-h-screen bg-white text-slate-900">
      {/* Navbar */}
      <nav className="sticky top-0 z-50 border-b border-slate-100 bg-white/80 backdrop-blur-lg">
        <div className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          <Link to="/" className="flex items-center gap-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-600 text-sm font-bold text-white">R</div>
            <span className="text-xl font-bold">RIVORA</span>
          </Link>
          <div className="hidden items-center gap-8 md:flex">
            <a href="#features" className="text-sm text-slate-600 transition hover:text-slate-900">Features</a>
            <a href="#pricing" className="text-sm text-slate-600 transition hover:text-slate-900">Pricing</a>
            <a href="#testimonials" className="text-sm text-slate-600 transition hover:text-slate-900">Témoignages</a>
          </div>
          <div className="flex items-center gap-3">
            <Link to="/app/login" className="text-sm font-medium text-slate-600 transition hover:text-slate-900">
              Connexion
            </Link>
            <Link to="/app/register" className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700">
              Essai gratuit
            </Link>
          </div>
        </div>
      </nav>

      {/* Hero */}
      <section className="relative overflow-hidden pt-20 pb-32">
        <div className="absolute inset-0 -z-10 bg-gradient-to-b from-blue-50/50 to-white" />
        <div className="absolute top-20 left-1/2 -z-10 h-[500px] w-[800px] -translate-x-1/2 rounded-full bg-blue-100/30 blur-3xl" />
        <div className="mx-auto max-w-4xl px-6 text-center">
          <div className="mb-6 inline-flex items-center gap-2 rounded-full border border-blue-200 bg-blue-50 px-4 py-1.5 text-sm text-blue-700">
            <Zap size={14} /> v4.0 Preview — .NET 9 + Clean Architecture
          </div>
          <h1 className="mb-6 text-5xl font-extrabold leading-tight tracking-tight text-slate-900 md:text-6xl">
            Le framework .NET pour{' '}
            <span className="bg-gradient-to-r from-blue-600 to-violet-600 bg-clip-text text-transparent">
              applications SaaS
            </span>
          </h1>
          <p className="mx-auto mb-10 max-w-2xl text-lg text-slate-600">
            Clean Architecture, multi-tenancy, sécurité enterprise et observabilité.
            Tout ce dont vous avez besoin pour lancer votre SaaS en semaines, pas en mois.
          </p>
          <div className="flex flex-wrap items-center justify-center gap-4">
            <Link
              to="/app/register"
              className="inline-flex items-center gap-2 rounded-xl bg-blue-600 px-6 py-3 text-sm font-semibold text-white shadow-lg shadow-blue-600/25 transition hover:bg-blue-700"
            >
              Démarrer gratuitement <ArrowRight size={16} />
            </Link>
            <a
              href="https://github.com/khalilbenaz/RIVORA"
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-2 rounded-xl border border-slate-300 px-6 py-3 text-sm font-semibold text-slate-700 transition hover:bg-slate-50"
            >
              <Code2 size={16} /> Voir sur GitHub
            </a>
          </div>
          <div className="mt-12 flex items-center justify-center gap-8 text-sm text-slate-500">
            <span className="flex items-center gap-1"><Layers size={14} /> Clean Architecture</span>
            <span className="flex items-center gap-1"><Globe size={14} /> Multi-tenant</span>
            <span className="flex items-center gap-1"><Shield size={14} /> Enterprise Security</span>
          </div>
        </div>
      </section>

      {/* Features */}
      <section id="features" className="py-24 bg-slate-50">
        <div className="mx-auto max-w-6xl px-6">
          <div className="mb-16 text-center">
            <h2 className="mb-4 text-3xl font-bold text-slate-900">Tout ce qu'il faut, rien de superflu</h2>
            <p className="mx-auto max-w-xl text-slate-600">
              Des modules production-ready que vous activez selon vos besoins. Pas de bloat.
            </p>
          </div>
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {features.map((f) => (
              <div key={f.title} className="rounded-xl border border-slate-200 bg-white p-6 transition hover:shadow-lg hover:border-blue-200">
                <div className="mb-4 flex h-11 w-11 items-center justify-center rounded-lg bg-blue-100 text-blue-600">
                  <f.icon size={22} />
                </div>
                <h3 className="mb-2 text-lg font-semibold text-slate-900">{f.title}</h3>
                <p className="text-sm leading-relaxed text-slate-600">{f.desc}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Code preview */}
      <section className="py-24">
        <div className="mx-auto max-w-6xl px-6">
          <div className="grid items-center gap-12 lg:grid-cols-2">
            <div>
              <h2 className="mb-4 text-3xl font-bold text-slate-900">Une commande. Un projet complet.</h2>
              <p className="mb-6 text-slate-600">
                Le CLI RIVORA génère une solution Clean Architecture avec tous les modules configurés,
                prête à déployer.
              </p>
              <ul className="space-y-3 text-sm text-slate-600">
                {['Domain, Application, Infrastructure, API', 'Auth JWT + RBAC configuré', 'Multi-tenancy prêt', 'Docker + CI/CD inclus'].map((item) => (
                  <li key={item} className="flex items-center gap-2">
                    <ChevronRight size={14} className="text-blue-500" /> {item}
                  </li>
                ))}
              </ul>
            </div>
            <div className="rounded-xl border border-slate-200 bg-slate-900 p-6 font-mono text-sm text-slate-300 shadow-2xl">
              <div className="mb-3 flex gap-1.5">
                <div className="h-3 w-3 rounded-full bg-red-500" />
                <div className="h-3 w-3 rounded-full bg-yellow-500" />
                <div className="h-3 w-3 rounded-full bg-green-500" />
              </div>
              <pre className="overflow-x-auto">
{`$ dotnet tool install --global RVR.CLI

$ rvr new MonSaaS --db postgresql \\
       --modules security,tenancy,jobs \\
       --auth jwt+2fa

✓ Solution MonSaaS créée avec succès
✓ 4 projets générés (Domain, App, Infra, API)
✓ Docker Compose configuré
✓ CI/CD GitHub Actions ajouté

$ cd MonSaaS && dotnet run
  → https://localhost:5001 🚀`}
              </pre>
            </div>
          </div>
        </div>
      </section>

      {/* Pricing */}
      <section id="pricing" className="py-24 bg-slate-50">
        <div className="mx-auto max-w-6xl px-6">
          <div className="mb-16 text-center">
            <h2 className="mb-4 text-3xl font-bold text-slate-900">Tarification simple</h2>
            <p className="text-slate-600">Open source au coeur. Support pro quand vous en avez besoin.</p>
          </div>
          <div className="grid gap-6 md:grid-cols-3">
            {pricing.map((plan) => (
              <div
                key={plan.name}
                className={`rounded-2xl border p-8 transition ${
                  plan.highlighted
                    ? 'border-blue-500 bg-white shadow-xl shadow-blue-100 ring-1 ring-blue-500'
                    : 'border-slate-200 bg-white hover:shadow-lg'
                }`}
              >
                {plan.highlighted && (
                  <div className="mb-4 inline-block rounded-full bg-blue-100 px-3 py-1 text-xs font-semibold text-blue-700">
                    Populaire
                  </div>
                )}
                <h3 className="text-xl font-bold text-slate-900">{plan.name}</h3>
                <div className="mt-4 mb-6">
                  <span className="text-4xl font-extrabold text-slate-900">{plan.price}</span>
                  {plan.period && <span className="text-slate-500"> {plan.period}</span>}
                </div>
                <ul className="mb-8 space-y-3">
                  {plan.features.map((f) => (
                    <li key={f} className="flex items-center gap-2 text-sm text-slate-600">
                      <ChevronRight size={14} className="text-emerald-500" /> {f}
                    </li>
                  ))}
                </ul>
                <Link
                  to="/app/register"
                  className={`block w-full rounded-lg py-2.5 text-center text-sm font-semibold transition ${
                    plan.highlighted
                      ? 'bg-blue-600 text-white hover:bg-blue-700'
                      : 'border border-slate-300 text-slate-700 hover:bg-slate-50'
                  }`}
                >
                  {plan.cta}
                </Link>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Testimonials */}
      <section id="testimonials" className="py-24">
        <div className="mx-auto max-w-6xl px-6">
          <div className="mb-16 text-center">
            <h2 className="mb-4 text-3xl font-bold text-slate-900">Ce qu'en disent nos utilisateurs</h2>
          </div>
          <div className="grid gap-6 md:grid-cols-3">
            {testimonials.map((t) => (
              <div key={t.name} className="rounded-xl border border-slate-200 bg-white p-6 transition hover:shadow-lg">
                <div className="mb-3 flex gap-0.5">
                  {Array.from({ length: t.rating }).map((_, i) => (
                    <Star key={i} size={16} className="fill-amber-400 text-amber-400" />
                  ))}
                </div>
                <p className="mb-4 text-sm leading-relaxed text-slate-600">"{t.text}"</p>
                <div>
                  <div className="font-semibold text-slate-900">{t.name}</div>
                  <div className="text-xs text-slate-500">{t.role}</div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA */}
      <section className="py-24 bg-gradient-to-r from-blue-600 to-violet-600">
        <div className="mx-auto max-w-3xl px-6 text-center">
          <h2 className="mb-4 text-3xl font-bold text-white">Prêt à lancer votre SaaS ?</h2>
          <p className="mb-8 text-lg text-blue-100">
            Rejoignez des centaines de développeurs qui construisent avec RIVORA.
          </p>
          <div className="flex flex-wrap justify-center gap-4">
            <Link
              to="/app/register"
              className="rounded-xl bg-white px-6 py-3 text-sm font-semibold text-blue-600 shadow-lg transition hover:bg-blue-50"
            >
              Créer mon compte gratuitement
            </Link>
            <a
              href="https://github.com/khalilbenaz/RIVORA"
              target="_blank"
              rel="noopener noreferrer"
              className="rounded-xl border border-white/30 px-6 py-3 text-sm font-semibold text-white transition hover:bg-white/10"
            >
              Voir le code source
            </a>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="border-t border-slate-200 bg-slate-50 py-12">
        <div className="mx-auto max-w-6xl px-6">
          <div className="grid gap-8 md:grid-cols-4">
            <div>
              <div className="mb-4 flex items-center gap-2">
                <div className="flex h-7 w-7 items-center justify-center rounded-lg bg-blue-600 text-xs font-bold text-white">R</div>
                <span className="font-bold">RIVORA</span>
              </div>
              <p className="text-sm text-slate-500">Clean Architecture Framework pour .NET 9. Open source.</p>
            </div>
            <div>
              <h4 className="mb-3 text-sm font-semibold text-slate-900">Produit</h4>
              <ul className="space-y-2 text-sm text-slate-500">
                <li><a href="#features" className="hover:text-slate-900">Features</a></li>
                <li><a href="#pricing" className="hover:text-slate-900">Pricing</a></li>
                <li><a href="https://khalilbenaz.github.io/RIVORA/" target="_blank" rel="noopener noreferrer" className="hover:text-slate-900">Documentation</a></li>
              </ul>
            </div>
            <div>
              <h4 className="mb-3 text-sm font-semibold text-slate-900">Développeurs</h4>
              <ul className="space-y-2 text-sm text-slate-500">
                <li><a href="https://github.com/khalilbenaz/RIVORA" target="_blank" rel="noopener noreferrer" className="hover:text-slate-900">GitHub</a></li>
                <li><Link to="/app/login" className="hover:text-slate-900">API Explorer</Link></li>
                <li><a href="https://www.nuget.org/packages?q=RVR.Framework" target="_blank" rel="noopener noreferrer" className="hover:text-slate-900">NuGet</a></li>
              </ul>
            </div>
            <div>
              <h4 className="mb-3 text-sm font-semibold text-slate-900">Légal</h4>
              <ul className="space-y-2 text-sm text-slate-500">
                <li><a href="#" className="hover:text-slate-900">Politique de confidentialité</a></li>
                <li><a href="#" className="hover:text-slate-900">CGU</a></li>
              </ul>
            </div>
          </div>
          <div className="mt-10 border-t border-slate-200 pt-6 text-center text-sm text-slate-400">
            &copy; {new Date().getFullYear()} RIVORA Framework. Tous droits réservés.
          </div>
        </div>
      </footer>
    </div>
  );
}
