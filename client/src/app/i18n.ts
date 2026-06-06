import enUS from '@/app/locales/en-US.json';
import ptBR from '@/app/locales/pt-BR.json';

const translations: Record<string, Record<string, string>> = {
  'en-US': enUS,
  'pt-BR': ptBR,
};

let currentLocale = 'pt-BR'; // Padrão

export const setLocale = (locale: string) => {
  if (translations[locale]) {
    currentLocale = locale;
    localStorage.setItem('locale', locale);
    window.dispatchEvent(new Event('languagechange'));
  }
};

export const getLocale = () => {
    return localStorage.getItem('locale') || currentLocale;
}

export const t = (key: string, interpolations?: Record<string, string | number>): string => {
  const locale = getLocale();
  let translation = translations[locale]?.[key] || key;

  if (interpolations) {
    Object.keys(interpolations).forEach((param) => {
      translation = translation.replace(`{{${param}}}`, String(interpolations[param]));
    });
  }

  return translation;
};
