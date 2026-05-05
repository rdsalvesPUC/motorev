import { authService } from './authService';
import { PATHS } from '../paths';
import { t } from '../i18n';
import { message } from 'antd';

let isRefreshing = false;
let failedQueue: { resolve: (token: string | null) => void; reject: (reason?: any) => void }[] = [];

const processQueue = (error: any | null, token: string | null = null) => {
  failedQueue.forEach(prom => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

export const tokenManager = {
  getAccessToken: (): string | null => localStorage.getItem('token'),
  getRefreshToken: (): string | null => localStorage.getItem('refreshToken'),
  getUserData: (): any | null => {
    const userData = localStorage.getItem('user');
    return userData ? JSON.parse(userData) : null;
  },
  getProfile: (): string | null => localStorage.getItem('perfil'),

  setTokens: (accessToken: string, refreshToken: string, userData: any, profile: string) => {
    localStorage.setItem('token', accessToken);
    localStorage.setItem('refreshToken', refreshToken);
    localStorage.setItem('user', JSON.stringify(userData));
    localStorage.setItem('perfil', profile);
  },

  clearTokens: () => {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    localStorage.removeItem('perfil');
  },

  async refreshAccessToken(): Promise<string | null> {
    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      });
    }

    isRefreshing = true;

    const refreshToken = tokenManager.getRefreshToken();
    const accessToken = tokenManager.getAccessToken();

    if (!refreshToken || !accessToken) {
      tokenManager.clearTokens();
      // Verificamos se estamos no browser antes de redirecionar para evitar erros em SSR (se aplicável)
      if (typeof window !== 'undefined') {
        window.location.href = PATHS.LOGIN;
      }
      return null;
    }

    try {
      const response = await authService.refreshToken(accessToken, refreshToken);
      const newAccessToken: string | null = response.token;
      const newRefreshToken: string | null = response.refreshToken;

      if (!newAccessToken || !newRefreshToken) {
        throw new Error('Invalid token response');
      }

      isRefreshing = false;
      processQueue(null, newAccessToken);
      
      // Atualiza os tokens no armazenamento
      const userData = tokenManager.getUserData();
      const profile = tokenManager.getProfile();
      tokenManager.setTokens(newAccessToken, newRefreshToken, userData, profile!);

      return newAccessToken;
    } catch (error) {
      isRefreshing = false;
      processQueue(error);
      message.error(t('auth.sessionExpired'));
      await authService.logout();
      if (typeof window !== 'undefined') {
        window.location.href = PATHS.LOGIN; // Garante o redirecionamento após limpar os dados
      }
      return null;
    }
  },
};
