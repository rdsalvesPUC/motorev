import { Routes, Route, Navigate } from 'react-router';
import DashboardLayout from './DashboardLayout';
import { Typography } from 'antd';
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

  return (
    <DashboardLayout
      userType="concessionaria"
      userName="Moto Center"
    >
      <Routes>
        <Route index element={<DashboardHome />} />
        <Route path="*" element={<Navigate to={PATHS.DASHBOARD_CONCESSIONARIA} replace />} />
      </Routes>
    </DashboardLayout>
  );
}
