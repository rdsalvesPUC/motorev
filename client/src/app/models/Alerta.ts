export type TipoAlerta =
  | 'RevisaoProxima'
  | 'RevisaoAtrasada'
  | 'AgendamentoCriado'
  | 'AgendamentoAlterado'
  | 'RevisaoConcluida';

export interface AlertaResponse {
  id: number;
  tipo: TipoAlerta;
  lido: boolean;
  criadoEm: string;
  motoId?: number;
  agendamentoId?: number;
  quilometragem?: number;
}

export interface AlertaNaoLidosCountResponse {
  total: number;
}
