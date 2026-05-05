import { apiFetch, handleResponse, BASE_URL } from './http';
import { Servico } from '../models/Servico';
import { t } from '../i18n';
import { ServicoRequest } from '../models/ServicoRequest';

const SERVICE_URL = `${BASE_URL}/Servico`;

export const servicoService = {
  getAll: async (categoria?: string): Promise<Servico[]> => {
    const url = new URL(SERVICE_URL);
    if (categoria) {
      url.searchParams.append('categoria', categoria);
    }
    
    const response = await apiFetch(url.toString());
    
    return handleResponse(response, t('error.fetchServices'));
  },

  getById: async (id: number): Promise<Servico> => {
    const response = await apiFetch(`${SERVICE_URL}/${id}`);
    
    return handleResponse(response, t('error.serviceNotFound'));
  },

  create: async (servico: ServicoRequest): Promise<Servico> => {
    const response = await apiFetch(SERVICE_URL, {
      method: 'POST',
      body: JSON.stringify(servico),
    });
    return handleResponse(response, t('error.createService'));
  },

  update: async (id: number, servico: ServicoRequest): Promise<Servico> => {
    const response = await apiFetch(`${SERVICE_URL}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(servico),
    });
    return handleResponse(response, t('error.updateService'));
  },

  delete: async (id: number): Promise<void> => {
    const response = await apiFetch(`${SERVICE_URL}/${id}`, {
      method: 'DELETE',
    });
    return handleResponse(response, t('error.deleteService'));
  },
};
