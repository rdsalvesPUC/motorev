import { BASE_URL, handleResponse, apiFetch } from './http';
import { tokenManager } from './tokenManager';

export const authService = {
  async login(data: any) {
    const response = await apiFetch(`${BASE_URL}/Auth/login`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha no login');
  },

  async refreshToken(accessToken: string, refreshToken: string) {
    const response = await apiFetch(`${BASE_URL}/Auth/refresh`, {
      method: 'POST',
      body: JSON.stringify({ accessToken, refreshToken }),
    });
    const data = await handleResponse(response, 'Falha ao renovar sessão');
    return data;
  },

  async logout() {
    try {
      const token = tokenManager.getAccessToken();
      // Tenta chamar o logout do backend apenas se existir um token
      if (token) {
        const response = await apiFetch(`${BASE_URL}/Auth/logout`, {
          method: 'POST',
        });
        await handleResponse(response, 'Falha no logout');
      }
    } finally {
      // Sempre limpa o armazenamento usando o tokenManager
      tokenManager.clearTokens();
    }
  }
};
