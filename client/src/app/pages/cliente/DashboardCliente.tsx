import { useCallback, useState } from 'react';
import { Routes, Route, Navigate } from 'react-router';
import DashboardLayout from '@/app/components/layout/DashboardLayout';
import MinhasMotos from '@/app/pages/cliente/motos/MinhasMotos';
import MotoForm from '@/app/pages/cliente/motos/MotoForm';
import MotoDetalhes from '@/app/pages/cliente/motos/MotoDetalhes';
import ConcessionariasCliente from '@/app/pages/cliente/concessionarias/Concessionarias';
import RevisoesCliente from '@/app/pages/cliente/revisoes/RevisoesCliente';
import AgendamentosCliente from '@/app/pages/cliente/agendamentos/AgendamentosCliente';
import PerfilCliente from '@/app/pages/cliente/perfil/PerfilCliente';
import {tokenManager} from "@/app/services/tokenManager";
import {PATH_SEGMENTS, PATHS} from "@/app/paths";

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
        <Route index element={<Navigate to={PATHS.CLIENTE_MOTOS} replace />} />
        <Route
          path={PATH_SEGMENTS.PERFIL_USUARIO}
          element={<PerfilCliente onProfileUpdated={handleProfileUpdated} />}
        />
        <Route path={PATH_SEGMENTS.CLIENTE_MOTOS} element={<MinhasMotos />} />
        <Route path={PATH_SEGMENTS.CLIENTE_MOTOS_NOVA} element={<MotoForm />} />
        <Route path={`${PATH_SEGMENTS.CLIENTE_MOTOS_EDITAR}/:id`} element={<MotoForm />} />
        <Route path={`${PATH_SEGMENTS.CLIENTE_MOTOS_DETALHES}/:id`} element={<MotoDetalhes />} />
        <Route path={PATH_SEGMENTS.CLIENTE_REVISOES} element={<RevisoesCliente />} />
        <Route path={PATH_SEGMENTS.CLIENTE_AGENDAMENTOS} element={<AgendamentosCliente />} />
        <Route path={PATH_SEGMENTS.CLIENTE_CONCESSIONARIAS} element={<ConcessionariasCliente />} />
        <Route path="*" element={<Navigate to={PATHS.CLIENTE_MOTOS} replace />} />
      </Routes>
    </DashboardLayout>
  );
}
