import { createContext, useContext, useEffect, useMemo, useState } from 'react';
import { getLocale, setLocale } from '@/app/i18n';

export type Idioma = 'pt-BR' | 'en-US';
export type Tema = 'light' | 'dark';

export interface Configuracoes {
  idioma: Idioma;
  tema: Tema;
}

interface ConfiguracoesContextType {
  configuracoes: Configuracoes;
  setIdioma: (idioma: Idioma) => void;
  setTema: (tema: Tema) => void;
  toggleTema: () => void;
}

const ConfiguracoesContext = createContext<ConfiguracoesContextType | null>(null);
const THEME_STORAGE_KEY = 'motorev_theme';

function getStoredTema(): Tema {
  const stored = localStorage.getItem(THEME_STORAGE_KEY);
  return stored === 'dark' ? 'dark' : 'light';
}

function getStoredIdioma(): Idioma {
  return getLocale() === 'en-US' ? 'en-US' : 'pt-BR';
}

export function ConfiguracoesProvider({ children }: { children: React.ReactNode }) {
  const [idioma, setIdiomaState] = useState<Idioma>(getStoredIdioma);
  const [tema, setTemaState] = useState<Tema>(getStoredTema);

  useEffect(() => {
    localStorage.setItem(THEME_STORAGE_KEY, tema);
    document.documentElement.dataset.theme = tema;
  }, [tema]);

  useEffect(() => {
    const handleLanguageChange = () => setIdiomaState(getStoredIdioma());
    window.addEventListener('languagechange', handleLanguageChange);
    return () => window.removeEventListener('languagechange', handleLanguageChange);
  }, []);

  const setIdioma = (nextIdioma: Idioma) => {
    setLocale(nextIdioma);
    setIdiomaState(nextIdioma);
  };

  const setTema = (nextTema: Tema) => {
    setTemaState(nextTema);
  };

  const toggleTema = () => {
    setTemaState((currentTema) => (currentTema === 'light' ? 'dark' : 'light'));
  };

  const value = useMemo<ConfiguracoesContextType>(
    () => ({
      configuracoes: { idioma, tema },
      setIdioma,
      setTema,
      toggleTema,
    }),
    [idioma, tema],
  );

  return (
    <ConfiguracoesContext.Provider value={value}>
      {children}
    </ConfiguracoesContext.Provider>
  );
}

export function useConfiguracoes() {
  const context = useContext(ConfiguracoesContext);

  if (!context) {
    throw new Error('useConfiguracoes must be used within ConfiguracoesProvider');
  }

  return context;
}
