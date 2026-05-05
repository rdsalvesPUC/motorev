import { Routes, Route, Navigate } from 'react-router';
import DashboardLayout from './DashboardLayout';
import { Typography } from 'antd';
import { tokenManager } from '../services/tokenManager';
import { PATHS, PATH_SEGMENTS } from '../paths';
import { t } from '../i18n';

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

  return (
    <DashboardLayout
      userType="cliente"
      userName={user?.nome || 'Usuário'}
    >
      <Routes>
        <Route index element={<DashboardHome />} />
        <Route path={PATH_SEGMENTS.CLIENTE_MOTOS} element={<DashboardHome />} />
        <Route path="*" element={<Navigate to={PATHS.DASHBOARD_CLIENTE} replace />} />
      </Routes>
    </DashboardLayout>
  );
}
