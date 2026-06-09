import assert from 'node:assert/strict';
import test from 'node:test';
import {
  isLojaPertoCliente,
  normalizarCidade,
  ordenarLojasPorProximidade,
} from './Concessionarias.utils.js';

test('normalizarCidade remove sufixo de UF e normaliza caixa/espacos', () => {
  assert.equal(normalizarCidade(' Sao Paulo - SP '), 'sao paulo');
  assert.equal(normalizarCidade('Curitiba'), 'curitiba');
});

test('isLojaPertoCliente identifica loja na mesma cidade do cliente', () => {
  assert.equal(isLojaPertoCliente({ cidade: 'Curitiba' }, 'Curitiba'), true);
  assert.equal(isLojaPertoCliente({ cidade: 'Curitiba' }, 'Curitiba - PR'), true);
  assert.equal(isLojaPertoCliente({ cidade: 'Pinhais' }, 'Curitiba'), false);
});

test('ordenarLojasPorProximidade prioriza lojas da cidade do cliente', () => {
  const lojas = [
    { id: 1, nome: 'Filial Sao Paulo', cidade: 'Sao Paulo' },
    { id: 2, nome: 'Filial Curitiba', cidade: 'Curitiba' },
    { id: 3, nome: 'Filial Pinhais', cidade: 'Pinhais' },
  ];

  const ordenadas = ordenarLojasPorProximidade(lojas, 'Curitiba - PR');

  assert.equal(ordenadas[0].id, 2);
  assert.deepEqual(lojas.map((loja) => loja.id), [1, 2, 3]);
});
