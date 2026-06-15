export type StatusAgendamentoConcessionaria =
  | 'aguardando_confirmacao'
  | 'agendada'
  | 'recusada'
  | 'em_execucao'
  | 'concluida'
  | 'cancelada';

export interface AgendamentoConcessionaria {
  agendamentoId: number;
  revisaoMotoId: number;
  motoId: number;
  lojaId: number;
  lojaNome: string;
  clienteNome: string;
  marca: string;
  modelo: string;
  placa: string;
  numeroRevisao: number;
  nomeRevisao: string;
  status: StatusAgendamentoConcessionaria;
  dataIdeal: string;
  dataAgendada: string;
  quilometragem: number;
  quantidadePecas: number;
  quantidadeServicos: number;
  mensagemRecusa?: string | null;
  dataRecusa?: string | null;
}
