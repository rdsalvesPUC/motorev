import { BASE_URL, apiFetch, handleResponse } from '@/app/services/http';
import { RevisaoPadraoListResponse, RevisaoPadraoResponse } from '@/app/models/RevisaoPadrao';
import { RevisaoPadraoLinhaRequest } from '@/app/models/RevisaoPadraoRequest';

const REVISAO_PADRAO_URL = `${BASE_URL}/revisao-padrao`;

export const revisaoPadraoService = {
  listar: async (params?: { modeloMotoId?: number; linhaId?: number }): Promise<RevisaoPadraoListResponse[]> => {
    const url = new URL(REVISAO_PADRAO_URL);

    if (params?.modeloMotoId) {
      url.searchParams.append('modeloMotoId', params.modeloMotoId.toString());
    }

    if (params?.linhaId) {
      url.searchParams.append('linhaId', params.linhaId.toString());
    }

    const response = await apiFetch(url.toString());
    return handleResponse(response, 'Erro ao carregar revisões padrão.');
  },

  criarPorLinha: async (payload: RevisaoPadraoLinhaRequest): Promise<RevisaoPadraoResponse[]> => {
    const response = await apiFetch(`${REVISAO_PADRAO_URL}/por-linha`, {
      method: 'POST',
      body: JSON.stringify(payload),
    });

    return handleResponse(response, 'Erro ao cadastrar revisões padrão por linha.');
  },

  alternarStatusPorLinha: async (linhaId: number): Promise<RevisaoPadraoListResponse[]> => {
    const response = await apiFetch(`${REVISAO_PADRAO_URL}/linha/${linhaId}/alternar-status`, {
      method: 'PATCH',
    });

    return handleResponse(response, 'Erro ao alternar status das revisões padrão.');
  },

  getById: async (id: number): Promise<RevisaoPadraoResponse> => {
    const response = await apiFetch(`${REVISAO_PADRAO_URL}/${id}`);
    return handleResponse(response, 'Erro ao carregar detalhes da revisão padrão.');
  },
};

