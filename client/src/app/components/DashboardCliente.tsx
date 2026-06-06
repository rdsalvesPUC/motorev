import { useCallback, useState } from 'react';
import { Routes, Route, Navigate } from 'react-router';
import DashboardLayout from './DashboardLayout';
import { Typography } from 'antd';
import { tokenManager } from '../services/tokenManager';
import { PATHS, PATH_SEGMENTS } from '../paths';
import { t } from '../i18n';
import PerfilCliente from './perfil/PerfilCliente';

const { Title, Paragraph } = Typography;

function DashboardHome() {
  return (
    <>
      <Title level={2}>{t('dashboard.welcome')}</Title>
      <Paragraph>
        {t('dashboard.clientAreaInfo')}
      </Paragraph>
    </>
  );
}

export default function DashboardCliente() {
  const user = tokenManager.getUserData();
  const [userName, setUserName] = useState(user?.nome || 'Usuário');
  const handleProfileUpdated = useCallback((perfil: { nome?: string | null }) => {
    setUserName(perfil.nome || 'Usuário');
  }, []);

  return (
    <DashboardLayout
      userType="cliente"
      userName={userName}
    >
      <Routes>
        <Route index element={<DashboardHome />} />
        <Route path={PATH_SEGMENTS.CLIENTE_MOTOS} element={<DashboardHome />} />
        <Route
          path={PATH_SEGMENTS.PERFIL_USUARIO}
          element={<PerfilCliente onProfileUpdated={handleProfileUpdated} />}
        />
        <Route path="*" element={<Navigate to={PATHS.DASHBOARD_CLIENTE} replace />} />
      </Routes>
    </DashboardLayout>
  );
}
