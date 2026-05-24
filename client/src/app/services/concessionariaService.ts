import { BASE_URL, handleResponse, apiFetch } from './http';
import { Concessionaria } from '../models/Concessionaria';

export const concessionariaService = {
  async register(data: any) {
    const response = await apiFetch(`${BASE_URL}/Concessionaria`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha no cadastro');
  },

  async getAll(): Promise<Concessionaria[]> {
    const response = await apiFetch(`${BASE_URL}/Concessionaria`);
    return handleResponse(response, 'Falha ao buscar concessionárias');
  }
};
