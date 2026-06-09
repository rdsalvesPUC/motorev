import { apiFetch, handleResponse, BASE_URL } from '@/app/services/http';
import { Moto, RevisaoMotoResponse } from '@/app/models/Moto';
import { MotoRequest } from '@/app/models/MotoRequest';
import { MotoUpdateRequest } from '@/app/models/MotoUpdateRequest';
import { t } from '@/app/i18n';

const MOTO_URL = `${BASE_URL}/Moto`;

export interface AgendamentoRevisaoRequest {
  lojaId: number;
  dataAgendamento: string;
}

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

  update: async (id: number, moto: MotoUpdateRequest): Promise<Moto> => {
    const response = await apiFetch(`${MOTO_URL}/${id}`, {
      method: 'PUT',
      body: JSON.stringify(moto),
    });
    return handleResponse(response, t('error.updateMoto'));
  },

  delete: async (id: number): Promise<void> => {
    const response = await apiFetch(`${MOTO_URL}/${id}`, {
      method: 'DELETE',
    });
    return handleResponse(response, t('error.deleteMoto'));
  },

  solicitarAgendamento: async (
    revisaoMotoId: number,
    request: AgendamentoRevisaoRequest,
  ): Promise<RevisaoMotoResponse> => {
    const response = await apiFetch(`${MOTO_URL}/revisoes/${revisaoMotoId}/agendamento`, {
      method: 'POST',
      body: JSON.stringify(request),
    });
    return handleResponse(response, 'Falha ao solicitar agendamento');
  },

  remarcarAgendamento: async (
    revisaoMotoId: number,
    request: AgendamentoRevisaoRequest,
  ): Promise<RevisaoMotoResponse> => {
    const response = await apiFetch(`${MOTO_URL}/revisoes/${revisaoMotoId}/agendamento`, {
      method: 'PUT',
      body: JSON.stringify(request),
    });
    return handleResponse(response, 'Falha ao remarcar agendamento');
  },

  cancelarAgendamento: async (revisaoMotoId: number): Promise<RevisaoMotoResponse> => {
    const response = await apiFetch(`${MOTO_URL}/revisoes/${revisaoMotoId}/agendamento`, {
      method: 'DELETE',
    });
    return handleResponse(response, 'Falha ao cancelar agendamento');
  },

  uploadImage: async (file: File): Promise<{ url: string }> => {
    const formData = new FormData();
    formData.append('file', file);
    const response = await apiFetch(`${BASE_URL}/Upload`, {
      method: 'POST',
      body: formData,
    });
    return handleResponse(response, t('motoForm.foto.error'));
  },
};
