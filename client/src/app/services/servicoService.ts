import { apiFetch, handleResponse, BASE_URL } from '@/app/services/http';
import { Servico } from '@/app/models/Servico';
import { t } from '@/app/i18n';
import { ServicoRequest } from '@/app/models/ServicoRequest';

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

  getCatalogo: async (categoria?: string, ativo?: boolean): Promise<Servico[]> => {
    const url = new URL(`${SERVICE_URL}/catalogo`);
    if (categoria) {
      url.searchParams.append('categoria', categoria);
    }
    if (ativo !== undefined) {
      url.searchParams.append('ativo', String(ativo));
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

  alternarStatus: async (id: number): Promise<Servico> => {
    const response = await apiFetch(`${SERVICE_URL}/${id}/alternar-status`, {
      method: 'PATCH',
    });
    return handleResponse(response, t('serviceCatalog.status.error'));
  },
};
