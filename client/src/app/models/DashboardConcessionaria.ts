export type StatusFilaRevisao = 'agendada' | 'em_execucao' | 'concluida';

export interface ItemFilaRevisao {
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
  status: StatusFilaRevisao;
  dataIdeal: string;
  dataAgendada: string;
  posicaoFila: number;
  quilometragem: number;
  quantidadePecas: number;
  quantidadeServicos: number;
  totalItens: number;
  itensConcluidos: number;
  progressoPercentual: number;
}

export interface FilaMecanico {
  mecanicoId: string;
  nome: string;
  especialidade: string;
  capacidadeDiaria: number;
  itens: ItemFilaRevisao[];
}

export interface DashboardConcessionariaRevisoes {
  data: string;
  totalMecanicos: number;
  totalRevisoes: number;
  revisoesAgendadas: number;
  revisoesEmExecucao: number;
  revisoesConcluidas: number;
  filas: FilaMecanico[];
}
