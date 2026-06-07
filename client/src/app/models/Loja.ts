export interface Loja {
  id: number;
  nome: string;
  tipo: string;
  cnpj: string;
  telefone: string;
  cep: string;
  logradouro: string;
  numero: string;
  bairro: string;
  cidade: string;
  uf: string;
  concessionariaId: number;
  ativo: boolean;
}
