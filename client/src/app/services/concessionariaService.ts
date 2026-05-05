import { BASE_URL, handleResponse, apiFetch } from './http';

export const concessionariaService = {
  async register(data: any) {
    const response = await apiFetch(`${BASE_URL}/Concessionaria`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha no cadastro');
  }
};
