import { BrowserRouter, Routes, Route } from 'react-router';
import MotoRevLandingPage from '../imports/MotoRevLandingPage';
import Login from './components/Login';
import Cadastro from './components/Cadastro';
import DashboardCliente from './components/DashboardCliente';
import DashboardConcessionaria from './components/DashboardConcessionaria';
import { AntdThemeProvider } from './theme';
import { ConfigProvider } from 'antd';
import { useEffect, useState } from 'react';
import { getLocale } from './i18n';
import ptBR from 'antd/locale/pt_BR';
import enUS from 'antd/locale/en_US';

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
            <Route path="/" element={<MotoRevLandingPage />} />
            <Route path="/login" element={<Login />} />
            <Route path="/cadastro" element={<Cadastro />} />
            <Route path="/dashboard/cliente/*" element={<DashboardCliente />} />
            <Route path="/dashboard/concessionaria/*" element={<DashboardConcessionaria />} />
          </Routes>
        </BrowserRouter>
      </ConfigProvider>
    </AntdThemeProvider>
  );
}
