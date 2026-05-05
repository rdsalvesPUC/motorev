import { Routes, Route, Navigate } from 'react-router';
import DashboardLayout from './DashboardLayout';
import { Typography } from 'antd';
import { PATHS } from '../paths';

const { Title, Paragraph } = Typography;

function DashboardHome() {
  return (
    <>
      <Title level={2}>Bem-vindo ao MotoRev</Title>
      <Paragraph>
        Esta é a área da concessionária. Use o menu lateral para navegar pelas funcionalidades.
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
