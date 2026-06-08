export interface ClienteEndereco {
  cep?: string | null;
  logradouro?: string | null;
  numero?: string | null;
  complemento?: string | null;
  bairro?: string | null;
  cidade?: string | null;
  uf?: string | null;
}

export interface ClientePerfil {
  id: number;
  nome: string;
  email: string;
  cpf: string;
  telefone?: string | null;
  endereco?: ClienteEndereco | null;
}

export interface ClienteDadosPessoaisRequest {
  nome: string;
  email: string;
  telefone: string;
}

export interface ClienteEnderecoRequest {
  cep?: string;
  logradouro?: string;
  numero?: string;
  complemento?: string;
  bairro?: string;
  cidade?: string;
  uf?: string;
}

export interface ClienteAlterarSenhaRequest {
  senhaAtual: string;
  novaSenha: string;
  confirmarNovaSenha: string;
}
