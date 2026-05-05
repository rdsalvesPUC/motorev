import { BASE_URL, handleResponse } from './http';

export const concessionariaService = {
  async register(data: any) {
    const response = await fetch(`${BASE_URL}/Concessionaria`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha no cadastro');
  }
};
