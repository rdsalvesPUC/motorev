import { Routes, Route, Navigate } from 'react-router';
import DashboardLayout from './DashboardLayout';
import { Typography } from 'antd';
import { PATHS, PATH_SEGMENTS } from '../paths';

const { Title, Paragraph } = Typography;

function DashboardHome() {
  return (
    <>
      <Title level={2}>Bem-vindo ao MotoRev</Title>
      <Paragraph>
        Esta é a área do cliente. Use o menu lateral para navegar pelas funcionalidades.
      </Paragraph>
    </>
  );
}

export default function DashboardCliente() {
  return (
    <DashboardLayout
      userType="cliente"
      userName="João Silva"
    >
      <Routes>
        <Route index element={<DashboardHome />} />
        <Route path={PATH_SEGMENTS.CLIENTE_MOTOS} element={<DashboardHome />} />
        <Route path="*" element={<Navigate to={PATHS.DASHBOARD_CLIENTE} replace />} />
      </Routes>
    </DashboardLayout>
  );
}
