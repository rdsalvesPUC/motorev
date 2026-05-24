import { Routes, Route, Navigate } from 'react-router';
import DashboardLayout from '@/app/components/layout/DashboardLayout';
import MinhasMotos from '@/app/pages/cliente/MinhasMotos';
import MotoForm from '@/app/pages/cliente/MotoForm';
import { Typography } from 'antd';
import {tokenManager} from "@/app/services/tokenManager";
import {PATH_SEGMENTS, PATHS} from "@/app/paths";
import { t } from '@/app/i18n';

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
        <Route path={PATH_SEGMENTS.CLIENTE_MOTOS} element={<MinhasMotos />} />
        <Route path={PATH_SEGMENTS.CLIENTE_MOTOS_NOVA} element={<MotoForm />} />
        <Route path={`${PATH_SEGMENTS.CLIENTE_MOTOS_EDITAR}/:id`} element={<MotoForm />} />
        <Route path="*" element={<Navigate to={PATHS.DASHBOARD_CLIENTE} replace />} />
      </Routes>
    </DashboardLayout>
  );
}
