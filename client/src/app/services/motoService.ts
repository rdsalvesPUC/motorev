import { apiFetch, handleResponse, BASE_URL } from './http';
import { Moto } from '../models/Moto';
import { MotoRequest } from '../models/MotoRequest';
import { t } from '../i18n';

const MOTO_URL = `${BASE_URL}/Moto`;

export const motoService = {
  create: async (moto: MotoRequest): Promise<Moto> => {
    const response = await apiFetch(MOTO_URL, {
      method: 'POST',
      body: JSON.stringify(moto),
    });
    return handleResponse(response, t('error.createMoto'));
  },

  uploadImage: async (file: File): Promise<{ url: string }> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiFetch(`${BASE_URL}/Upload`, {
      method: 'POST',
      body: formData,
    });
    return handleResponse(response, 'Falha ao fazer upload da imagem');
  },
};
