import { BASE_URL, handleResponse, apiFetch } from '@/app/services/http';

export const clienteService = {
  async register(data: any) {
    const response = await apiFetch(`${BASE_URL}/Cliente`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha no cadastro');
  }
};
