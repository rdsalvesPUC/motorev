export interface ModeloMotoRequest {
  nomeModelo: string;
  marca: string;
  categoria?: string;
  linhaId: number;
  cilindrada?: string;
  ano?: number;
}
