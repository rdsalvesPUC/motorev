import { apiFetch, handleResponse, BASE_URL } from '@/app/services/http';
import { Moto } from '@/app/models/Moto';
import { MotoRequest } from '@/app/models/MotoRequest';
import { t } from '@/app/i18n';

const MOTO_URL = `${BASE_URL}/Moto`;

export const motoService = {
  getAll: async (): Promise<Moto[]> => {
    const response = await apiFetch(MOTO_URL, {
      method: 'GET',
    });
    return handleResponse(response, t('error.listMotos'));
  },

  create: async (moto: MotoRequest): Promise<Moto> => {
    const response = await apiFetch(MOTO_URL, {
      method: 'POST',
      body: JSON.stringify(moto),
    });
    return handleResponse(response, t('error.createMoto'));
  },

  getById: async (id: number): Promise<Moto> => {
    const response = await apiFetch(`${MOTO_URL}/${id}`, {
      method: 'GET',
    });
    return handleResponse(response, t('error.getMoto'));
  },

  update: async (id: number, moto: MotoRequest): Promise<Moto> => {
    const response = await apiFetch(`${MOTO_URL}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(moto),
    });
    return handleResponse(response, t('error.updateMoto'));
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
