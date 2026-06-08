export interface RevisaoPadraoPecaRequest {
  pecaId: number;
  quantidade: number;
}

export interface RevisaoPadraoLinhaItemRequest {
  nome: string;
  ordem: number;
  quilometragem: number;
  tempoMeses: number;
  servicosIds: number[];
  pecas?: RevisaoPadraoPecaRequest[];
}

export interface RevisaoPadraoLinhaRequest {
  nome: string;
  linhaId: number;
  revisoes: RevisaoPadraoLinhaItemRequest[];
}

