import { RevisaoPadraoPecaResponse } from '@/app/models/RevisaoPadrao';
import { Servico } from '@/app/models/Servico';

export interface RevisaoMotoResponse {
  id: number;
  revisaoPadraoId: number;
  nome: string;
  ordem: number;
  quilometragem: number;
  tempoMeses: number;
  dataPrevista: string;
  status: string;
  servicos: Servico[];
  pecas: RevisaoPadraoPecaResponse[];
}

export interface Moto {
  id: number;
  placa: string;
  chassi: string;
  modeloMotoId: number;
  nomeModelo: string;
  marca: string;
  clienteId: number;
  concessionariaId?: number;
  nomeConcessionaria?: string;
  foto?: string;
  cor: string;
  kilometragemAtual: number;
  dataVenda: string;
  linha: string;
  cilindrada: string;
  ano: number;
  revisoesPlanejadas: RevisaoMotoResponse[];
}
