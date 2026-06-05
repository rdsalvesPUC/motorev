import { BASE_URL, handleResponse, apiFetch } from './http';

export type CategoriaPeca = 'Filtros' | 'Motor' | 'Freios' | 'Transmissão' | 'Elétrica';
export type StatusPecaFilter = 'Todos' | 'Ativo' | 'Inativo';
export type StatusCadastro = 'Ativo' | 'Inativo';

export interface PecaResponse {
  id: number;
  codigo: string;
  nome: string;
  categoria: CategoriaPeca | string;
  preco: number;
  estoque: number;
  status: StatusCadastro | string;
}

export interface PecaRequest {
  codigo: string;
  nome: string;
  categoria: CategoriaPeca;
  preco: number;
  estoque: number;
}

export interface PecaUpdateRequest extends PecaRequest {
  status: StatusCadastro;
}

export const CATEGORIAS_PECA: Array<{ value: CategoriaPeca; label: string }> = [
  { value: 'Filtros', label: 'Filtros' },
  { value: 'Motor', label: 'Motor' },
  { value: 'Freios', label: 'Freios' },
  { value: 'Transmissão', label: 'Transmissão' },
  { value: 'Elétrica', label: 'Elétrica' },
];

export const pecaService = {
  async listar(status: StatusPecaFilter = 'Todos'): Promise<PecaResponse[]> {
    const query = status === 'Todos' ? '' : `?status=${encodeURIComponent(status)}`;
    const response = await apiFetch(`${BASE_URL}/Peca/listar${query}`);
    return handleResponse(response, 'Falha ao carregar peças');
  },

  async criar(data: PecaRequest): Promise<PecaResponse> {
    const response = await apiFetch(`${BASE_URL}/Peca/criar`, {
      method: 'POST',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha ao cadastrar peça');
  },

  async atualizar(id: number, data: PecaUpdateRequest): Promise<PecaResponse> {
    const response = await apiFetch(`${BASE_URL}/Peca/id/${id}`, {
      method: 'PUT',
      body: JSON.stringify(data),
    });
    return handleResponse(response, 'Falha ao atualizar peça');
  },
};
