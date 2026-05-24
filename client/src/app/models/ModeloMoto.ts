export interface ModeloMoto {
  id: number;
  nomeModelo: string;
  marca: string;
  categoria?: string;
  linha?: string;
  cilindrada?: string;
  ano?: number;
  ativo: boolean;
}
