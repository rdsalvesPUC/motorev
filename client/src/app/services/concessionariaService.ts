import { BASE_URL, handleResponse, apiFetch } from '@/app/services/http';
import { Concessionaria } from '@/app/models/Concessionaria';
import { Loja } from '@/app/models/Loja';
import { LojaRequest } from '@/app/models/LojaRequest';
import { ConcessionariaPerfilRequest } from '@/app/models/ConcessionariaPerfilRequest';
import { ConcessionariaAlterarSenhaRequest } from '@/app/models/ConcessionariaAlterarSenhaRequest';

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

  async updateMe(data: ConcessionariaPerfilRequest): Promise<Concessionaria> {
    const response = await apiFetch(`${BASE_URL}/Concessionaria/me`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha ao atualizar concessionaria');
  },

  async alterarSenha(data: ConcessionariaAlterarSenhaRequest): Promise<void> {
    const response = await apiFetch(`${BASE_URL}/Concessionaria/me/senha`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    await handleResponse(response, 'Falha ao alterar senha');
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
