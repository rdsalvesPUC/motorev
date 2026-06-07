export interface ViaCepAddress {
  cep: string;
  logradouro: string;
  bairro: string;
  cidade: string;
  uf: string;
}

interface ViaCepResponse {
  cep?: string;
  logradouro?: string;
  bairro?: string;
  localidade?: string;
  uf?: string;
  erro?: boolean;
}

export const viaCepService = {
  async getAddressByCep(cep: string): Promise<ViaCepAddress> {
    const digits = cep.replace(/\D/g, '');
    if (digits.length !== 8) {
      throw new Error('CEP invalido');
    }

    const response = await fetch(`https://viacep.com.br/ws/${digits}/json/`);
    if (!response.ok) {
      throw new Error('Falha ao buscar CEP');
    }

    const data: ViaCepResponse = await response.json();
    if (data.erro) {
      throw new Error('CEP nao encontrado');
    }

    return {
      cep: data.cep ?? cep,
      logradouro: data.logradouro ?? '',
      bairro: data.bairro ?? '',
      cidade: data.localidade ?? '',
      uf: data.uf ?? '',
    };
  },
};
