import { useState } from 'react';
import { Link, Navigate } from 'react-router-dom';
import { useAuthStore } from '../../store/authStore';
import { authApi } from '../../api/auth';

export default function ClientLogin() {
  const { isAuthenticated, login } = useAuthStore();
  const [userName, setUserName] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  if (isAuthenticated) return <Navigate to="/app" replace />;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const res = await authApi.login({ userName, password });
      login(res.data.token, res.data.user);
    } catch {
      setError('Identifiants incorrects. Vérifiez votre nom d\'utilisateur et mot de passe.');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="flex min-h-screen">
      {/* Left - Form */}
      <div className="flex w-full items-center justify-center px-6 lg:w-1/2">
        <div className="w-full max-w-md">
          <Link to="/" className="mb-8 flex items-center gap-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-600 text-sm font-bold text-white">R</div>
            <span className="text-xl font-bold text-slate-900">RIVORA</span>
          </Link>

          <h1 className="mb-2 text-2xl font-bold text-slate-900">Bon retour !</h1>
          <p className="mb-8 text-sm text-slate-500">Connectez-vous à votre espace.</p>

          <form onSubmit={handleSubmit} className="space-y-5">
            {error && (
              <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
                {error}
              </div>
            )}

            <div>
              <label htmlFor="userName" className="mb-1.5 block text-sm font-medium text-slate-700">
                Nom d'utilisateur
              </label>
              <input
                id="userName"
                type="text"
                value={userName}
                onChange={(e) => setUserName(e.target.value)}
                required
                autoFocus
                className="w-full rounded-lg border border-slate-300 px-3.5 py-2.5 text-sm transition focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              />
            </div>

            <div>
              <div className="mb-1.5 flex items-center justify-between">
                <label htmlFor="password" className="text-sm font-medium text-slate-700">
                  Mot de passe
                </label>
                <a href="#" className="text-xs text-blue-600 hover:underline">Oublié ?</a>
              </div>
              <input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
                className="w-full rounded-lg border border-slate-300 px-3.5 py-2.5 text-sm transition focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-500/20"
              />
            </div>

            <button
              type="submit"
              disabled={loading}
              className="w-full rounded-lg bg-blue-600 py-2.5 text-sm font-semibold text-white transition hover:bg-blue-700 disabled:opacity-60"
            >
              {loading ? 'Connexion...' : 'Se connecter'}
            </button>
          </form>

          <p className="mt-6 text-center text-sm text-slate-500">
            Pas encore de compte ?{' '}
            <Link to="/app/register" className="font-medium text-blue-600 hover:underline">
              S'inscrire gratuitement
            </Link>
          </p>
        </div>
      </div>

      {/* Right - Visual */}
      <div className="hidden items-center justify-center bg-gradient-to-br from-blue-600 to-violet-600 lg:flex lg:w-1/2">
        <div className="max-w-md px-12 text-white">
          <h2 className="mb-4 text-3xl font-bold">Construisez votre SaaS avec RIVORA</h2>
          <p className="text-blue-100">
            Clean Architecture, multi-tenancy et sécurité enterprise.
            Tout ce qu'il faut pour réussir.
          </p>
        </div>
      </div>
    </div>
  );
}
