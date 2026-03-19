import { useAuthStore } from '../../store/authStore';
import { Link } from 'react-router-dom';
import { Code2, Book, Settings, Key, BarChart3, Shield } from 'lucide-react';

const quickActions = [
  { icon: Code2, title: 'API Explorer', desc: 'Testez les endpoints de votre API', href: '/swagger', external: true },
  { icon: Book, title: 'Documentation', desc: 'Guides et référence API complète', href: 'https://khalilbenaz.github.io/RIVORA/', external: true },
  { icon: Key, title: 'Clés API', desc: 'Gérez vos clés d\'accès API', href: '/app/settings' },
  { icon: BarChart3, title: 'Métriques', desc: 'Consultez vos statistiques d\'usage', href: '/app/settings' },
  { icon: Shield, title: 'Sécurité', desc: 'Activez le 2FA et gérez vos sessions', href: '/app/settings' },
  { icon: Settings, title: 'Paramètres', desc: 'Configurez votre compte', href: '/app/settings' },
];

export default function ClientDashboard() {
  const user = useAuthStore((s) => s.user);

  return (
    <div>
      {/* Welcome */}
      <div className="mb-8 rounded-2xl bg-gradient-to-r from-blue-600 to-violet-600 p-8 text-white shadow-lg">
        <h1 className="text-2xl font-bold">
          Bienvenue, {user?.firstName || user?.userName} !
        </h1>
        <p className="mt-2 text-blue-100">
          Votre espace RIVORA est prêt. Explorez vos outils ci-dessous.
        </p>
      </div>

      {/* Quick Actions */}
      <h2 className="mb-4 text-lg font-semibold text-slate-800">Actions rapides</h2>
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {quickActions.map((action) => {
          const Card = (
            <div className="group flex gap-4 rounded-xl border border-slate-200 bg-white p-5 transition hover:border-blue-300 hover:shadow-md">
              <div className="flex h-11 w-11 flex-shrink-0 items-center justify-center rounded-lg bg-blue-50 text-blue-600 transition group-hover:bg-blue-100">
                <action.icon size={22} />
              </div>
              <div>
                <h3 className="font-semibold text-slate-900">{action.title}</h3>
                <p className="mt-0.5 text-sm text-slate-500">{action.desc}</p>
              </div>
            </div>
          );

          if (action.external) {
            return (
              <a key={action.title} href={action.href} target="_blank" rel="noopener noreferrer">
                {Card}
              </a>
            );
          }
          return <Link key={action.title} to={action.href}>{Card}</Link>;
        })}
      </div>

      {/* Getting Started */}
      <div className="mt-8 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
        <h2 className="mb-4 text-lg font-semibold text-slate-800">Pour commencer</h2>
        <div className="space-y-4">
          {[
            { step: 1, title: 'Installer le CLI', cmd: 'dotnet tool install --global RVR.CLI' },
            { step: 2, title: 'Créer un projet', cmd: 'rvr new MonApp --db postgresql --modules security,tenancy' },
            { step: 3, title: 'Lancer', cmd: 'cd MonApp && dotnet run' },
          ].map((item) => (
            <div key={item.step} className="flex items-start gap-4">
              <div className="flex h-8 w-8 flex-shrink-0 items-center justify-center rounded-full bg-blue-100 text-sm font-bold text-blue-600">
                {item.step}
              </div>
              <div className="flex-1">
                <div className="mb-1 font-medium text-slate-900">{item.title}</div>
                <code className="block rounded-lg bg-slate-900 px-4 py-2.5 text-sm text-slate-300">
                  {item.cmd}
                </code>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
