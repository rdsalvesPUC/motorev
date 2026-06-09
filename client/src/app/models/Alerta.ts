export type TipoAlerta =
  | 'RevisaoProxima'
  | 'RevisaoAtrasada'
  | 'AgendamentoCriado'
  | 'AgendamentoAlterado'
  | 'RevisaoConcluida'
  | 'AgendamentoAprovado'
  | 'AgendamentoRecusado'
  | 'NovaSolicitacao'
  | 'Cancelamento'
  | 'Reagendamento';

export interface AlertaResponse {
  id: number;
  tipo: TipoAlerta;
  lido: boolean;
  criadoEm: string;
  motoId?: number;
  agendamentoId?: number;
  quilometragem?: number;
  ordemRevisao?: number;
  modeloMotoNome?: string;
  marcaMoto?: string;
}

export interface AlertaNaoLidosCountResponse {
  total: number;
}
