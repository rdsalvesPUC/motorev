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
}
