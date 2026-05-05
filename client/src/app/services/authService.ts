import { BASE_URL, handleResponse } from './http';

export const authService = {
  async login(data: any) {
    const response = await fetch(`${BASE_URL}/Auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha no login');
  }
};
