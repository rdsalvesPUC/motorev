import { apiFetch, handleResponse, BASE_URL } from '@/app/services/http';
import { ModeloMoto } from '@/app/models/ModeloMoto';
import { ModeloMotoRequest } from '@/app/models/ModeloMotoRequest';
import { t } from '@/app/i18n';

const MODELO_MOTO_URL = `${BASE_URL}/ModeloMoto`;

export const modeloMotoService = {
  getAll: async (): Promise<ModeloMoto[]> => {
    const response = await apiFetch(`${MODELO_MOTO_URL}/listar`);
    return handleResponse(response, t('error.fetchModelosMotos'));
  },

  getById: async (id: number): Promise<ModeloMoto> => {
    const response = await apiFetch(`${MODELO_MOTO_URL}/id/${id}`);
    return handleResponse(response, t('error.modeloMotoNotFound'));
  },

  create: async (modeloMoto: ModeloMotoRequest): Promise<ModeloMoto> => {
    const response = await apiFetch(`${MODELO_MOTO_URL}/criar`, {
      method: 'POST',
      body: JSON.stringify(modeloMoto),
    });
    return handleResponse(response, t('error.createModeloMoto'));
  },

  update: async (id: number, modeloMoto: ModeloMotoRequest): Promise<ModeloMoto> => {
    const response = await apiFetch(`${MODELO_MOTO_URL}/atualizar/${id}`, {
      method: 'PUT',
      body: JSON.stringify(modeloMoto),
    });
    return handleResponse(response, t('error.updateModeloMoto'));
  },

  alternarStatus: async (id: number): Promise<ModeloMoto> => {
    const response = await apiFetch(`${MODELO_MOTO_URL}/alternar-status/${id}`, {
      method: 'PATCH',
    });
    const result = await handleResponse(response, t('error.toggleModeloMotoStatus'));
    return result.modelo ?? result;
  },
};
