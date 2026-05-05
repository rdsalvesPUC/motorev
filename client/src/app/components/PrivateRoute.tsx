import { Navigate } from 'react-router';
import { PATHS } from '../paths';

interface PrivateRouteProps {
  children: React.ReactElement;
  allowedProfile: 'Cliente' | 'Concessionaria';
}

export default function PrivateRoute({ children, allowedProfile }: PrivateRouteProps) {
  const token = localStorage.getItem('token');
  const userProfile = localStorage.getItem('perfil');

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
