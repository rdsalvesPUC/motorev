import { apiFetch, handleResponse, BASE_URL } from './http';
import { ModeloMoto } from '../models/ModeloMoto';
import { t } from '../i18n';

const MODELO_MOTO_URL = `${BASE_URL}/ModeloMoto`;

export const modeloMotoService = {
  getAll: async (): Promise<ModeloMoto[]> => {
    const response = await apiFetch(`${MODELO_MOTO_URL}/listar`);
    return handleResponse(response, t('error.fetchModelosMotos'));
  },
};
