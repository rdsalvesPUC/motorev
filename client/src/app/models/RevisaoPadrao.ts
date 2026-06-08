import { Servico } from '@/app/models/Servico';

export interface RevisaoPadraoPecaResponse {
  id: number;
  codigo: string;
  nome: string;
  categoria: string;
  preco: number;
  estoque: number;
  status: string;
  quantidade: number;
}

export interface RevisaoPadraoListResponse {
  id: number;
  nome: string;
  modeloMotoId: number;
  nomeModeloMoto: string;
  linhaId: number;
  nomeLinha: string;
  ordem: number;
  quilometragem: number;
  tempoMeses: number;
  ativo: boolean;
}

export interface RevisaoPadraoResponse {
  id: number;
  nome: string;
  ordem: number;
  quilometragem: number;
  tempoMeses: number;
  modeloMotoId: number;
  nomeModeloMoto: string;
  servicos: Servico[];
  pecas: RevisaoPadraoPecaResponse[];
}

