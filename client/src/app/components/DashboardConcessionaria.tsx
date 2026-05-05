import { Routes, Route, Navigate } from 'react-router';
import DashboardLayout from './DashboardLayout';
import { Typography } from 'antd';
import { tokenManager } from '../services/tokenManager';
import { PATHS } from '../paths';
import { t } from '../i18n';

const { Title, Paragraph } = Typography;

function DashboardHome() {
  return (
    <>
      <Title level={2}>{t('dashboard.welcome')}</Title>
      <Paragraph>
        {t('dashboard.dealershipAreaInfo')}
      </Paragraph>
    </>
  );
}

export default function DashboardConcessionaria() {
  const user = tokenManager.getUserData();

  return (
    <DashboardLayout
      userType="concessionaria"
      userName={user?.nome || 'Concessionária'}
    >
      <Routes>
        <Route index element={<DashboardHome />} />
        <Route path="*" element={<Navigate to={PATHS.DASHBOARD_CONCESSIONARIA} replace />} />
      </Routes>
    </DashboardLayout>
  );
}
