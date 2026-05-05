import { useEffect } from 'react';
import { useNavigate } from 'react-router';
import { PATHS } from '../paths';

interface PublicRouteProps {
  children: React.ReactElement;
}

export default function PublicRoute({ children }: PublicRouteProps) {
  const navigate = useNavigate();

  useEffect(() => {
    const token = localStorage.getItem('token');
    const perfil = localStorage.getItem('perfil');

    if (token && perfil) {
      const dashboardPath = perfil === 'Cliente' ? PATHS.DASHBOARD_CLIENTE : PATHS.DASHBOARD_CONCESSIONARIA;
      navigate(dashboardPath);
    }
  }, [navigate]);

  const isAuthenticated = !!localStorage.getItem('token') && !!localStorage.getItem('perfil');

  return isAuthenticated ? null : children;
}
