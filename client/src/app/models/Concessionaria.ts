import { Loja } from '@/app/models/Loja';

export interface Concessionaria {
  id: number;
  nome: string;
  email: string;
  cnpj: string;
  telefone: string;
  tipo: string;
  cep: string;
  logradouro: string;
  numero: string;
  bairro: string;
  cidade: string;
  uf: string;
  lojas: Loja[];
}
