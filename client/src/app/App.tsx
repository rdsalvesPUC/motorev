import { BrowserRouter, Routes, Route } from 'react-router';
import MotoRevLandingPage from '@/imports/MotoRevLandingPage';
import Login from '@/app/pages/auth/Login';
import Cadastro from '@/app/pages/auth/Cadastro';
import DashboardCliente from '@/app/pages/cliente/DashboardCliente';
import DashboardConcessionaria from '@/app/pages/concessionaria/DashboardConcessionaria';
import AccessDenied from '@/app/pages/auth/AccessDenied';
import { AntdThemeProvider } from '@/app/theme';
import { ConfigProvider, Spin } from 'antd';
import { useEffect, useState } from 'react';
import { getLocale } from '@/app/i18n';
import ptBR from 'antd/locale/pt_BR';
import enUS from 'antd/locale/en_US';
import PublicRoute from '@/app/components/auth/PublicRoute';
import PrivateRoute from '@/app/components/auth/PrivateRoute';
import { PATHS } from '@/app/paths';
import { tokenManager } from '@/app/services/tokenManager';
import { t } from '@/app/i18n';
import { message } from 'antd';

const antdLocales: Record<string, any> = {
  'pt-BR': ptBR,
  'en-US': enUS,
};

export default function App() {
  const [locale, setLocale] = useState(antdLocales[getLocale()]);
  const [initializing, setInitializing] = useState(true);

  useEffect(() => {
    const currentLang = getLocale();
    document.documentElement.lang = currentLang;

    const handleLanguageChange = () => {
      const newLang = getLocale();
      setLocale(antdLocales[newLang]);
      document.documentElement.lang = newLang;
    };
    window.addEventListener('languagechange', handleLanguageChange);

    // Validação de sessão na inicialização
    const validateSession = async () => {
      const token = tokenManager.getAccessToken();
      const refreshToken = tokenManager.getRefreshToken();

      if (token && refreshToken) {
        try {
          // Tenta renovar o token para garantir que a sessão ainda é válida
          await tokenManager.refreshAccessToken();
        } catch (error) {
          console.error('Session validation failed:', error);
          // Em caso de erro, garantimos que a inicialização termine para que o roteamento aconteça
          setInitializing(false);
          return;
        }
      }
      setInitializing(false);
    };

    validateSession();

    return () => window.removeEventListener('languagechange', handleLanguageChange);
  }, []);

  if (initializing) {
    return (
      <AntdThemeProvider>
        <ConfigProvider locale={locale}>
          <div style={{ height: '100vh', display: 'flex', justifyContent: 'center', alignItems: 'center' }}>
            <Spin size="large" />
          </div>
        </ConfigProvider>
      </AntdThemeProvider>
    );
  }

  return (
    <AntdThemeProvider>
      <ConfigProvider locale={locale}>
        <BrowserRouter>
          <Routes>
            <Route path={PATHS.HOME} element={<MotoRevLandingPage />} />
            <Route path={PATHS.LOGIN} element={<PublicRoute><Login /></PublicRoute>} />
            <Route path={PATHS.CADASTRO} element={<PublicRoute><Cadastro /></PublicRoute>} />
            <Route path={PATHS.ACCESS_DENIED} element={<AccessDenied />} />
            
            {/* Rotas Protegidas*/}
            <Route 
              path={`${PATHS.DASHBOARD_CLIENTE}/*`} 
              element={<PrivateRoute allowedProfile="Cliente"><DashboardCliente /></PrivateRoute>} 
            />
            <Route 
              path={`${PATHS.DASHBOARD_CONCESSIONARIA}/*`} 
              element={<PrivateRoute allowedProfile="Concessionaria"><DashboardConcessionaria /></PrivateRoute>}
            />
          </Routes>
        </BrowserRouter>
      </ConfigProvider>
    </AntdThemeProvider>
  );
}
