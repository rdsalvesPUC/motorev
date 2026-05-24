export interface MotoRequest {
  placa: string;
  chassi: string;
  modeloMotoId: number;
  concessionariaId?: number;
  foto?: string;
  cor: string;
  kilometragemAtual: number;
  dataVenda: string;
}
