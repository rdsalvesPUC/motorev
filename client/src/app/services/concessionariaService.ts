import { BASE_URL, handleResponse, apiFetch } from '@/app/services/http';
import { Concessionaria } from '@/app/models/Concessionaria';
import { Loja } from '@/app/models/Loja';
import { LojaRequest } from '@/app/models/LojaRequest';

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
    return handleResponse(response, 'Falha ao buscar concessionarias');
  },

  async getMe(): Promise<Concessionaria> {
    const response = await apiFetch(`${BASE_URL}/Concessionaria/me`);
    return handleResponse(response, 'Falha ao buscar concessionaria autenticada');
  },

  async getLojas(concessionariaId: number): Promise<Loja[]> {
    const response = await apiFetch(`${BASE_URL}/Concessionaria/${concessionariaId}/lojas`);
    return handleResponse(response, 'Falha ao buscar lojas');
  },

  async getLojaById(concessionariaId: number, lojaId: number): Promise<Loja> {
    const response = await apiFetch(`${BASE_URL}/Concessionaria/${concessionariaId}/lojas/${lojaId}`);
    return handleResponse(response, 'Falha ao buscar loja');
  },

  async createLoja(concessionariaId: number, data: LojaRequest): Promise<Loja> {
    const response = await apiFetch(`${BASE_URL}/Concessionaria/${concessionariaId}/lojas`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha ao criar loja');
  },

  async updateLoja(concessionariaId: number, lojaId: number, data: LojaRequest): Promise<Loja> {
    const response = await apiFetch(`${BASE_URL}/Concessionaria/${concessionariaId}/lojas/${lojaId}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha ao atualizar loja');
  },

  async alternarStatusLoja(concessionariaId: number, lojaId: number): Promise<Loja> {
    const response = await apiFetch(`${BASE_URL}/Concessionaria/${concessionariaId}/lojas/${lojaId}/alternar-status`, {
      method: 'PATCH',
    });
    return handleResponse(response, 'Falha ao alterar status da loja');
  },
};
