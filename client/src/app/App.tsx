import { BrowserRouter, Routes, Route } from 'react-router';
import MotoRevLandingPage from '../imports/MotoRevLandingPage';
import Login from './components/Login';
import Cadastro from './components/Cadastro';
import DashboardCliente from './components/DashboardCliente';
import DashboardConcessionaria from './components/DashboardConcessionaria';
import { AntdThemeProvider } from './theme';

export default function App() {
  return (
    <AntdThemeProvider>
      <BrowserRouter>
        <Routes>
          <Route path="/" element={<MotoRevLandingPage />} />
          <Route path="/login" element={<Login />} />
          <Route path="/cadastro" element={<Cadastro />} />
          <Route path="/dashboard/cliente/*" element={<DashboardCliente />} />
          <Route path="/dashboard/concessionaria/*" element={<DashboardConcessionaria />} />
        </Routes>
      </BrowserRouter>
    </AntdThemeProvider>
  );
}
