import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import api from '../../api/client';
import FormField from '../../components/FormField';

interface FieldErrors {
  firstName?: string;
  lastName?: string;
  userName?: string;
  email?: string;
  password?: string;
}

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

function validate(form: { userName: string; email: string; password: string; firstName: string; lastName: string }): FieldErrors {
  const errors: FieldErrors = {};
  if (!form.firstName.trim()) errors.firstName = 'Le prénom est requis.';
  if (!form.lastName.trim()) errors.lastName = 'Le nom est requis.';
  if (!form.userName.trim()) errors.userName = "Le nom d'utilisateur est requis.";
  if (!form.email.trim()) errors.email = "L'email est requis.";
  else if (!EMAIL_RE.test(form.email)) errors.email = "Format d'email invalide.";
  if (!form.password) errors.password = 'Le mot de passe est requis.';
  else if (form.password.length < 8) errors.password = 'Minimum 8 caractères.';
  return errors;
}

export default function ClientRegister() {
  const navigate = useNavigate();
  const [form, setForm] = useState({ userName: '', email: '', password: '', firstName: '', lastName: '' });
  const [error, setError] = useState('');
  const [fieldErrors, setFieldErrors] = useState<FieldErrors>({});
  const [loading, setLoading] = useState(false);
  const [touched, setTouched] = useState(false);

  const update = (field: string, value: string) => {
    const next = { ...form, [field]: value };
    setForm(next);
    if (touched) setFieldErrors(validate(next));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setTouched(true);
    const errors = validate(form);
    setFieldErrors(errors);
    if (Object.keys(errors).length > 0) return;
    setError('');
    setLoading(true);
    try {
      await api.post('/users', form);
      navigate('/app/login');
    } catch {
      setError('Erreur lors de l\'inscription. Vérifiez les informations saisies.');
    } finally {
      setLoading(false);
    }
  };

  const hasErrors = Object.keys(fieldErrors).length > 0;

  return (
    <div className="flex min-h-screen">
      <div className="flex w-full items-center justify-center px-6 lg:w-1/2">
        <div className="w-full max-w-md">
          <Link to="/" className="mb-8 flex items-center gap-2">
            <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-blue-600 text-sm font-bold text-white">R</div>
            <span className="text-xl font-bold text-slate-900">RIVORA</span>
          </Link>

          <h1 className="mb-2 text-2xl font-bold text-slate-900">Créer un compte</h1>
          <p className="mb-8 text-sm text-slate-500">Commencez gratuitement, pas de carte bancaire requise.</p>

          <form onSubmit={handleSubmit} className="space-y-4">
            {error && (
              <div className="rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">
                {error}
              </div>
            )}

            <div className="grid grid-cols-2 gap-3">
              <FormField label="Prénom" value={form.firstName} onChange={(v) => update('firstName', v)} required error={fieldErrors.firstName} />
              <FormField label="Nom" value={form.lastName} onChange={(v) => update('lastName', v)} required error={fieldErrors.lastName} />
            </div>

            <FormField label="Nom d'utilisateur" value={form.userName} onChange={(v) => update('userName', v)} required error={fieldErrors.userName} />

            <FormField label="Email" type="email" value={form.email} onChange={(v) => update('email', v)} required error={fieldErrors.email} />

            <div>
              <FormField label="Mot de passe" type="password" value={form.password} onChange={(v) => update('password', v)} required error={fieldErrors.password} />
              {!fieldErrors.password && <p className="mt-1 text-xs text-slate-400">Minimum 8 caractères</p>}
            </div>

            <button
              type="submit"
              disabled={loading || hasErrors}
              className="w-full rounded-lg bg-blue-600 py-2.5 text-sm font-semibold text-white transition hover:bg-blue-700 disabled:opacity-60"
            >
              {loading ? 'Inscription...' : 'Créer mon compte'}
            </button>

            <p className="text-center text-xs text-slate-400">
              En vous inscrivant, vous acceptez nos{' '}
              <a href="#" className="text-blue-600 hover:underline">CGU</a> et{' '}
              <a href="#" className="text-blue-600 hover:underline">politique de confidentialité</a>.
            </p>
          </form>

          <p className="mt-6 text-center text-sm text-slate-500">
            Déjà un compte ?{' '}
            <Link to="/app/login" className="font-medium text-blue-600 hover:underline">
              Se connecter
            </Link>
          </p>
        </div>
      </div>

      <div className="hidden items-center justify-center bg-gradient-to-br from-violet-600 to-blue-600 lg:flex lg:w-1/2">
        <div className="max-w-md px-12 text-white">
          <h2 className="mb-4 text-3xl font-bold">Lancez-vous en quelques minutes</h2>
          <ul className="space-y-3 text-blue-100">
            <li className="flex items-center gap-2">&#10003; Clean Architecture prête à l'emploi</li>
            <li className="flex items-center gap-2">&#10003; Multi-tenancy out-of-the-box</li>
            <li className="flex items-center gap-2">&#10003; Sécurité enterprise</li>
            <li className="flex items-center gap-2">&#10003; Support communautaire gratuit</li>
          </ul>
        </div>
      </div>
    </div>
  );
}
