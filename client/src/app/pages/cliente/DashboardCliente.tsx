import { useCallback, useState } from 'react';
import { Routes, Route, Navigate } from 'react-router';
import DashboardLayout from '@/app/components/layout/DashboardLayout';
import MinhasMotos from '@/app/pages/cliente/MinhasMotos';
import MotoForm from '@/app/pages/cliente/MotoForm';
import MotoDetalhes from '@/app/pages/cliente/MotoDetalhes';
import ConcessionariasCliente from '@/app/pages/cliente/Concessionarias';
import PerfilCliente from '@/app/components/perfil/PerfilCliente';
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
        <Route
          path={PATH_SEGMENTS.PERFIL_USUARIO}
          element={<PerfilCliente onProfileUpdated={handleProfileUpdated} />}
        />
        <Route path={PATH_SEGMENTS.CLIENTE_MOTOS} element={<MinhasMotos />} />
        <Route path={PATH_SEGMENTS.CLIENTE_MOTOS_NOVA} element={<MotoForm />} />
        <Route path={`${PATH_SEGMENTS.CLIENTE_MOTOS_EDITAR}/:id`} element={<MotoForm />} />
        <Route path={`${PATH_SEGMENTS.CLIENTE_MOTOS_DETALHES}/:id`} element={<MotoDetalhes />} />
        <Route path={PATH_SEGMENTS.CLIENTE_CONCESSIONARIAS} element={<ConcessionariasCliente />} />
        <Route path="*" element={<Navigate to={PATHS.DASHBOARD_CLIENTE} replace />} />
      </Routes>
    </DashboardLayout>
  );
}
