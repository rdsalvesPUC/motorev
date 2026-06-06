export interface ModeloMoto {
  id: number;
  nomeModelo: string;
  marca: string;
  categoria?: string;
  linhaId: number;
  cilindrada?: string;
  ano?: number;
  ativo: boolean;
}
