import { useTranslation } from 'react-i18next';

export default function LanguageSwitcher() {
  const { i18n } = useTranslation();

  const switchLang = (lng: string) => {
    i18n.changeLanguage(lng);
    localStorage.setItem('rvr_lang', lng);
  };

  return (
    <div className="flex items-center gap-1 text-xs">
      <button
        onClick={() => switchLang('fr')}
        className={`rounded px-2 py-1 font-medium transition-colors ${
          i18n.language === 'fr'
            ? 'bg-blue-600 text-white'
            : 'text-slate-400 hover:text-slate-200'
        }`}
      >
        FR
      </button>
      <span className="text-slate-600">|</span>
      <button
        onClick={() => switchLang('en')}
        className={`rounded px-2 py-1 font-medium transition-colors ${
          i18n.language === 'en'
            ? 'bg-blue-600 text-white'
            : 'text-slate-400 hover:text-slate-200'
        }`}
      >
        EN
      </button>
    </div>
  );
}
