import { apiFetch, handleResponse, BASE_URL } from '@/app/services/http';
import { Linha } from '@/app/models/Linha';
import { LinhaRequest } from '@/app/models/LinhaRequest';
import { t } from '@/app/i18n';

const LINHA_URL = `${BASE_URL}/Linha`;

export const linhaService = {
  getAll: async (apenasAtivos: boolean = true): Promise<Linha[]> => {
    const response = await apiFetch(`${LINHA_URL}?apenasAtivos=${apenasAtivos}`);
    return handleResponse(response, t('error.fetchLinhas'));
  },

  getById: async (id: number): Promise<Linha> => {
    const response = await apiFetch(`${LINHA_URL}/${id}`);
    return handleResponse(response, t('error.linhaNotFound'));
  },

  create: async (linha: LinhaRequest): Promise<Linha> => {
    const response = await apiFetch(LINHA_URL, {
      method: 'POST',
      body: JSON.stringify(linha),
    });
    return handleResponse(response, t('error.createLinha'));
  },

  update: async (id: number, linha: LinhaRequest): Promise<Linha> => {
    const response = await apiFetch(`${LINHA_URL}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(linha),
    });
    return handleResponse(response, t('error.updateLinha'));
  },

  delete: async (id: number): Promise<Linha> => {
    const response = await apiFetch(`${LINHA_URL}/${id}`, {
      method: 'DELETE',
    });
    return handleResponse(response, t('error.deleteLinha'));
  },

  alternarStatus: async (id: number): Promise<Linha> => {
    const response = await apiFetch(`${LINHA_URL}/alternar-status/${id}`, {
      method: 'PATCH',
    });
    return handleResponse(response, t('error.toggleLinhaStatus'));
  },
};
