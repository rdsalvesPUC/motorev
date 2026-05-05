import enUS from './locales/en-US.json';
import ptBR from './locales/pt-BR.json';

const translations: Record<string, Record<string, string>> = {
  'en-US': enUS,
  'pt-BR': ptBR,
};

let currentLocale = 'pt-BR'; // Default

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

export const t = (key: string): string => {
  const locale = getLocale();
  return translations[locale]?.[key] || key;
};
