import { Routes, Route, Navigate } from 'react-router';
import DashboardLayout from './DashboardLayout';
import { Typography } from 'antd';

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
        <Route path="motos" element={<DashboardHome />} />
        <Route path="*" element={<Navigate to="/dashboard/cliente" replace />} />
      </Routes>
    </DashboardLayout>
  );
}
