import { useEffect } from 'react';
import { useNavigate } from 'react-router';
import { PATHS } from '../paths';
import { tokenManager } from '../services/tokenManager';

interface PublicRouteProps {
  children: React.ReactElement;
}

export default function PublicRoute({ children }: PublicRouteProps) {
  const navigate = useNavigate();

  useEffect(() => {
    const token = tokenManager.getAccessToken();
    const perfil = tokenManager.getProfile();

    if (token && perfil) {
      const dashboardPath = perfil === 'Cliente' ? PATHS.DASHBOARD_CLIENTE : PATHS.DASHBOARD_CONCESSIONARIA;
      navigate(dashboardPath);
    }
  }, [navigate]);

  const isAuthenticated = !!tokenManager.getAccessToken() && !!tokenManager.getProfile();

  return isAuthenticated ? null : children;
}
