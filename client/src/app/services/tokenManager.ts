import { authService } from './authService';
import { PATHS } from '../paths';
import { t } from '../i18n';
import { message } from 'antd';
import { BASE_URL, handleResponse } from './http';

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

    try {
      const refreshToken = tokenManager.getRefreshToken();
      const accessToken = tokenManager.getAccessToken();

      if (!refreshToken || !accessToken) {
        tokenManager.clearTokens();
        if (typeof window !== 'undefined') {
          window.location.href = PATHS.LOGIN;
        }
        
        // Importante: resetar flag e processar fila com erro antes de retornar
        isRefreshing = false;
        processQueue(new Error('Missing tokens'));
        return null;
      }

      const data = await authService.refreshToken(accessToken, refreshToken);
      const newAccessToken: string | null = data.token;
      const newRefreshToken: string | null = data.refreshToken;

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
      tokenManager.clearTokens();
      
      message.error(t('auth.sessionExpired'));
      
      if (typeof window !== 'undefined') {
        window.location.href = PATHS.LOGIN;
      }
      return null;
    }
  },
};
