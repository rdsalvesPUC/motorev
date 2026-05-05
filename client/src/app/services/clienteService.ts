import { BASE_URL, handleResponse } from './http';

export const clienteService = {
  async register(data: any) {
    const response = await fetch(`${BASE_URL}/Cliente`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha no cadastro');
  }
};
