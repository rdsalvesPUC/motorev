interface ViaCepResponse {
  cep?: string;
  logradouro?: string;
  complemento?: string;
  bairro?: string;
  localidade?: string;
  uf?: string;
  erro?: boolean | string;
}

export interface ViaCepEndereco {
  cep: string;
  logradouro: string;
  complemento: string;
  bairro: string;
  cidade: string;
  uf: string;
}

export const viaCepService = {
  async buscarEnderecoPorCep(cep: string): Promise<ViaCepEndereco | null> {
    const digits = cep.replace(/\D/g, '');
    if (digits.length !== 8) {
      throw new Error('CEP inválido');
    }

    const response = await fetch(`https://viacep.com.br/ws/${digits}/json/`);
    if (!response.ok) {
      throw new Error('Falha ao consultar CEP');
    }

    const data = (await response.json()) as ViaCepResponse;
    if (data.erro) {
      return null;
    }

    return {
      cep: data.cep || digits,
      logradouro: data.logradouro || '',
      complemento: data.complemento || '',
      bairro: data.bairro || '',
      cidade: data.localidade || '',
      uf: data.uf || '',
    };
  },
};
