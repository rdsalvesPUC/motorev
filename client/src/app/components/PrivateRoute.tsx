import { Navigate } from 'react-router';
import { PATHS } from '../paths';
import { tokenManager } from '../services/tokenManager';

interface PrivateRouteProps {
  children: React.ReactElement;
  allowedProfile: 'Cliente' | 'Concessionaria';
}

export default function PrivateRoute({ children, allowedProfile }: PrivateRouteProps) {
  const token = tokenManager.getAccessToken();
  const userProfile = tokenManager.getProfile();

  if (!token || !userProfile) {
    // Usuario não autenticado, redireciona para login
    return <Navigate to={PATHS.LOGIN} replace />;
  }

  if (userProfile !== allowedProfile) {
    // Usuario autenticado, mas com perfil incorreto, redireciona para Acesso Negado
    return <Navigate to={PATHS.ACCESS_DENIED} replace />;
  }

  // Usuario autenticado e com perfil correto, renderiza rota normalmente
  return children;
}
