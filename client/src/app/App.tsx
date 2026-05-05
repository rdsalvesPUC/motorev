import { BrowserRouter, Routes, Route } from 'react-router';
import MotoRevLandingPage from '../imports/MotoRevLandingPage';
import Login from './components/Login';
import Cadastro from './components/Cadastro';
import DashboardCliente from './components/DashboardCliente';
import DashboardConcessionaria from './components/DashboardConcessionaria';
import AccessDenied from './components/AccessDenied'; // Import AccessDenied
import { AntdThemeProvider } from './theme';
import { ConfigProvider } from 'antd';
import { useEffect, useState } from 'react';
import { getLocale } from './i18n';
import ptBR from 'antd/locale/pt_BR';
import enUS from 'antd/locale/en_US';
import PublicRoute from './components/PublicRoute';
import PrivateRoute from './components/PrivateRoute';
import { PATHS } from './paths';

const antdLocales: Record<string, any> = {
  'pt-BR': ptBR,
  'en-US': enUS,
};

export default function App() {
  const [locale, setLocale] = useState(antdLocales[getLocale()]);

  useEffect(() => {
    const handleLanguageChange = () => {
      setLocale(antdLocales[getLocale()]);
    };
    window.addEventListener('languagechange', handleLanguageChange);
    return () => window.removeEventListener('languagechange', handleLanguageChange);
  }, []);

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
