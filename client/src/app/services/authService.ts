import { BASE_URL, handleResponse } from './http';

export const authService = {
  async login(data: any) {
    const response = await fetch(`${BASE_URL}/Auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha no login');
  },

  async logout() {
    try {
      const token = localStorage.getItem('token');
      const response = await fetch(`${BASE_URL}/Auth/logout`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${token}`
        },
      });
      await handleResponse(response, 'Falha no logout');
    } finally {
      // Always clear local storage, regardless of API call success or failure
      localStorage.removeItem('token');
      localStorage.removeItem('refreshToken');
      localStorage.removeItem('user');
      localStorage.removeItem('perfil');
    }
  }
};
