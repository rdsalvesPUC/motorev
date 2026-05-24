import { apiFetch, handleResponse, BASE_URL } from '@/app/services/http';
import { ModeloMoto } from '@/app/models/ModeloMoto';
import { t } from '@/app/i18n';

const MODELO_MOTO_URL = `${BASE_URL}/ModeloMoto`;

export const modeloMotoService = {
  getAll: async (): Promise<ModeloMoto[]> => {
    const response = await apiFetch(`${MODELO_MOTO_URL}/listar`);
    return handleResponse(response, t('error.fetchModelosMotos'));
  },
};
