import { BASE_URL, handleResponse, apiFetch } from './http';
import type {
  ClienteAlterarSenhaRequest,
  ClienteDadosPessoaisRequest,
  ClienteEnderecoRequest,
  ClientePerfil,
} from '../models/ClientePerfil';

export const clienteService = {
  async register(data: any) {
    const response = await apiFetch(`${BASE_URL}/Cliente`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha no cadastro');
  },

  async getPerfil(): Promise<ClientePerfil> {
    const response = await apiFetch(`${BASE_URL}/Cliente/me`);
    return handleResponse(response, 'Falha ao carregar perfil');
  },

  async updateDadosPessoais(data: ClienteDadosPessoaisRequest): Promise<ClientePerfil> {
    const response = await apiFetch(`${BASE_URL}/Cliente/me/dados-pessoais`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha ao atualizar dados pessoais');
  },

  async updateEndereco(data: ClienteEnderecoRequest): Promise<ClientePerfil> {
    const response = await apiFetch(`${BASE_URL}/Cliente/me/endereco`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha ao atualizar endereço');
  },

  async alterarSenha(data: ClienteAlterarSenhaRequest): Promise<void> {
    const response = await apiFetch(`${BASE_URL}/Cliente/me/senha`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    await handleResponse(response, 'Falha ao alterar senha');
  },
};
