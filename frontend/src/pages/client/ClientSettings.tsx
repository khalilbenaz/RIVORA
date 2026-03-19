import { useState } from 'react';
import { useAuthStore } from '../../store/authStore';
import { User, Shield, Key, Bell } from 'lucide-react';
import PushNotificationToggle from '../../components/PushNotificationToggle';

type Tab = 'profile' | 'security' | 'api' | 'notifications';

export default function ClientSettings() {
  const user = useAuthStore((s) => s.user);
  const [activeTab, setActiveTab] = useState<Tab>('profile');

  const tabs: { id: Tab; label: string; icon: React.ElementType }[] = [
    { id: 'profile', label: 'Profil', icon: User },
    { id: 'security', label: 'Sécurité', icon: Shield },
    { id: 'api', label: 'Clés API', icon: Key },
    { id: 'notifications', label: 'Notifications', icon: Bell },
  ];

  return (
    <div>
      <h1 className="mb-6 text-2xl font-bold text-slate-900">Paramètres</h1>

      <div className="flex gap-6">
        {/* Tabs */}
        <nav className="w-48 flex-shrink-0">
          <ul className="space-y-1">
            {tabs.map(({ id, label, icon: Icon }) => (
              <li key={id}>
                <button
                  onClick={() => setActiveTab(id)}
                  className={`flex w-full items-center gap-2.5 rounded-lg px-3 py-2 text-sm font-medium transition ${
                    activeTab === id
                      ? 'bg-blue-50 text-blue-700'
                      : 'text-slate-600 hover:bg-slate-100'
                  }`}
                >
                  <Icon size={16} /> {label}
                </button>
              </li>
            ))}
          </ul>
        </nav>

        {/* Content */}
        <div className="flex-1 rounded-xl border border-slate-200 bg-white p-6 shadow-sm">
          {activeTab === 'profile' && (
            <div>
              <h2 className="mb-4 text-lg font-semibold text-slate-800">Profil</h2>
              <div className="max-w-md space-y-4">
                <div>
                  <label className="mb-1 block text-sm font-medium text-slate-700">Nom d'utilisateur</label>
                  <input type="text" defaultValue={user?.userName} disabled className="w-full rounded-lg border border-slate-200 bg-slate-50 px-3 py-2.5 text-sm text-slate-500" />
                </div>
                <div className="grid grid-cols-2 gap-3">
                  <div>
                    <label className="mb-1 block text-sm font-medium text-slate-700">Prénom</label>
                    <input type="text" defaultValue={user?.firstName ?? ''} className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20" />
                  </div>
                  <div>
                    <label className="mb-1 block text-sm font-medium text-slate-700">Nom</label>
                    <input type="text" defaultValue={user?.lastName ?? ''} className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20" />
                  </div>
                </div>
                <div>
                  <label className="mb-1 block text-sm font-medium text-slate-700">Email</label>
                  <input type="email" defaultValue={user?.email} className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20" />
                </div>
                <button className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700">
                  Sauvegarder
                </button>
              </div>
            </div>
          )}

          {activeTab === 'security' && (
            <div>
              <h2 className="mb-4 text-lg font-semibold text-slate-800">Sécurité</h2>
              <div className="max-w-md space-y-6">
                <div className="rounded-lg border border-slate-200 p-4">
                  <div className="flex items-center justify-between">
                    <div>
                      <h3 className="font-medium text-slate-900">Authentification à deux facteurs</h3>
                      <p className="text-sm text-slate-500">{user?.twoFactorEnabled ? 'Activée' : 'Non activée'}</p>
                    </div>
                    <button className={`rounded-lg px-3 py-1.5 text-sm font-medium ${user?.twoFactorEnabled ? 'border border-red-200 text-red-600 hover:bg-red-50' : 'bg-emerald-600 text-white hover:bg-emerald-700'}`}>
                      {user?.twoFactorEnabled ? 'Désactiver' : 'Activer'}
                    </button>
                  </div>
                </div>
                <div>
                  <h3 className="mb-3 font-medium text-slate-900">Changer le mot de passe</h3>
                  <div className="space-y-3">
                    <input type="password" placeholder="Mot de passe actuel" className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20" />
                    <input type="password" placeholder="Nouveau mot de passe" className="w-full rounded-lg border border-slate-300 px-3 py-2.5 text-sm focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20" />
                    <button className="rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700">
                      Mettre à jour
                    </button>
                  </div>
                </div>
              </div>
            </div>
          )}

          {activeTab === 'api' && (
            <div>
              <h2 className="mb-4 text-lg font-semibold text-slate-800">Clés API</h2>
              <p className="mb-4 text-sm text-slate-500">Gérez vos clés d'accès à l'API RIVORA.</p>
              <div className="rounded-lg border border-dashed border-slate-300 py-12 text-center">
                <Key size={32} className="mx-auto mb-3 text-slate-300" />
                <p className="text-sm text-slate-500">Aucune clé API créée.</p>
                <button className="mt-3 rounded-lg bg-blue-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-blue-700">
                  Générer une clé
                </button>
              </div>
            </div>
          )}

          {activeTab === 'notifications' && (
            <div>
              <h2 className="mb-4 text-lg font-semibold text-slate-800">Notifications</h2>
              <div className="space-y-4">
                <PushNotificationToggle />
                {[
                  { label: 'Alertes de sécurité', desc: 'Connexions suspectes et changements de mot de passe', checked: true },
                  { label: 'Mises à jour produit', desc: 'Nouvelles fonctionnalités et releases', checked: true },
                  { label: 'Newsletter', desc: 'Actualités et bonnes pratiques', checked: false },
                ].map((item) => (
                  <div key={item.label} className="flex items-center justify-between rounded-lg border border-slate-200 p-4">
                    <div>
                      <h3 className="font-medium text-slate-900">{item.label}</h3>
                      <p className="text-sm text-slate-500">{item.desc}</p>
                    </div>
                    <input type="checkbox" defaultChecked={item.checked} className="h-5 w-5 accent-blue-600" />
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
