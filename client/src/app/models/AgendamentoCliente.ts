export type StatusAgendamentoCliente =
  | 'aguardando_agendamento'
  | 'aguardando_confirmacao'
  | 'agendada'
  | 'em_execucao'
  | 'atrasada';

export interface AgendamentoCliente {
  agendamentoId?: number | null;
  revisaoMotoId: number;
  motoId: number;
  marca: string;
  modelo: string;
  placa: string;
  numeroRevisao: number;
  nomeRevisao: string;
  status: StatusAgendamentoCliente;
  dataIdeal: string;
  dataMinima: string;
  dataLimite: string;
  dataAgendada?: string | null;
  lojaId?: number | null;
  nomeLoja?: string | null;
  cidadeLoja?: string | null;
  quantidadePecas: number;
  quantidadeServicos: number;
  prazoTexto: string;
  mensagemRecusa?: string | null;
  dataRecusa?: string | null;
}
