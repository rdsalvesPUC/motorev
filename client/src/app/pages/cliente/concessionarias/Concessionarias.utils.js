export function normalizarCidade(cidade) {
  return (cidade ?? '').split(' - ')[0].trim().toLowerCase();
}

export function cidadeUf(loja) {
  return `${loja.cidade} - ${loja.uf}`;
}

export function isLojaPertoCliente(loja, clienteCidade) {
  return Boolean(clienteCidade) && normalizarCidade(loja.cidade) === normalizarCidade(clienteCidade);
}

export function ordenarLojasPorProximidade(lojas, clienteCidade) {
  return lojas
    .slice()
    .sort((a, b) => Number(isLojaPertoCliente(b, clienteCidade)) - Number(isLojaPertoCliente(a, clienteCidade)));
}
