import assert from 'node:assert/strict';
import test from 'node:test';
import {
  REVISION_STATUS,
  EXECUTION_ITEM_STATUS,
  agruparRevisoesDasMotos,
  applyRevisionStatusOverride,
  buildMotoRevisionDetailsPath,
  calcularResumoRevisoes,
  filtrarRevisoes,
  getExecutionItemStatus,
  getExecutionProgress,
  getRevisionEstimate,
  getRevisionStatus,
  ordenarRevisoes,
} from './RevisoesCliente.utils.js';

const today = new Date('2026-06-09T12:00:00');

const motos = [
  {
    id: 10,
    placa: 'ABC1234',
    marca: 'Honda',
    nomeModelo: 'Titan 160',
    ano: 2025,
    cor: 'Preta',
    linha: 'CG',
    kilometragemAtual: 1500,
    revisoesPlanejadas: [
      {
        id: 1,
        nome: 'Primeira revisao',
        ordem: 1,
        quilometragem: 1000,
        tempoMeses: 6,
        dataPrevista: '2026-05-01T00:00:00',
        status: 'Planejada',
        servicos: [{ id: 1, nome: 'Troca de oleo', custo: 80, tempoEstimado: 30 }],
        pecas: [{ id: 1, nome: 'Filtro', preco: 45, quantidade: 2 }],
      },
      {
        id: 2,
        nome: 'Segunda revisao',
        ordem: 2,
        quilometragem: 5000,
        tempoMeses: 12,
        dataPrevista: '2026-08-01T00:00:00',
        status: 'Agendada',
        servicos: [],
        pecas: [],
      },
    ],
  },
  {
    id: 20,
    placa: 'XYZ9876',
    marca: 'Yamaha',
    nomeModelo: 'Fazer 250',
    ano: 2024,
    cor: 'Azul',
    linha: 'Fazer',
    kilometragemAtual: 8000,
    revisoesPlanejadas: [
      {
        id: 3,
        nome: 'Terceira revisao',
        ordem: 3,
        quilometragem: 10000,
        tempoMeses: 18,
        dataPrevista: '2026-12-01T00:00:00',
        status: 'Concluida',
        servicos: [{ id: 2, nome: 'Inspecao', custo: 120, tempoEstimado: 45 }],
        pecas: [],
      },
    ],
  },
];

test('getRevisionStatus marca planejada vencida como atrasada', () => {
  const status = getRevisionStatus(motos[0].revisoesPlanejadas[0], today);

  assert.equal(status, REVISION_STATUS.ATRASADA);
});

test('getRevisionStatus marca revisao dentro da tolerancia como aguardando agendamento', () => {
  const status = getRevisionStatus(
    {
      dataPrevista: '2026-06-18T00:00:00',
      status: 'Planejada',
    },
    new Date('2026-06-14T12:00:00')
  );

  assert.equal(status, REVISION_STATUS.AGUARDANDO_AGENDAMENTO);
});

test('getRevisionStatus mantem planejada quando ainda nao entrou na tolerancia', () => {
  const status = getRevisionStatus(
    {
      dataPrevista: '2026-07-10T00:00:00',
      status: 'Planejada',
    },
    new Date('2026-06-14T12:00:00')
  );

  assert.equal(status, REVISION_STATUS.PLANEJADA);
});

test('agruparRevisoesDasMotos achata revisoes preservando contexto da moto', () => {
  const revisoes = agruparRevisoesDasMotos(motos, today);

  assert.equal(revisoes.length, 3);
  assert.equal(revisoes[0].motoId, 10);
  assert.equal(revisoes[0].moto.nomeModelo, 'Titan 160');
  assert.equal(revisoes[0].status, REVISION_STATUS.ATRASADA);
  assert.equal(revisoes[0].totalEstimado, 170);
  assert.equal(revisoes[0].totalTempo, 30);
});

test('calcularResumoRevisoes contabiliza os status exibidos na pagina', () => {
  const revisoes = agruparRevisoesDasMotos(motos, today);
  const resumo = calcularResumoRevisoes(revisoes);

  assert.deepEqual(resumo, {
    total: 3,
    concluidas: 1,
    pendentes: 0,
    agendadas: 1,
    emExecucao: 0,
    atrasadas: 1,
  });
});

test('filtrarRevisoes filtra por moto, status e busca textual', () => {
  const revisoes = agruparRevisoesDasMotos(motos, today);

  assert.equal(filtrarRevisoes(revisoes, { motoId: 20 }).length, 1);
  assert.equal(filtrarRevisoes(revisoes, { status: REVISION_STATUS.ATRASADA }).length, 1);
  assert.equal(filtrarRevisoes(revisoes, { busca: 'fazer' }).length, 1);
  assert.equal(filtrarRevisoes(revisoes, { busca: '1234' }).length, 2);
});

test('ordenarRevisoes prioriza atrasadas e mantem ordenacao por data', () => {
  const revisoes = agruparRevisoesDasMotos(motos, today);
  const ordenadas = ordenarRevisoes(revisoes);

  assert.equal(ordenadas[0].status, REVISION_STATUS.ATRASADA);
  assert.equal(ordenadas.at(-1).status, REVISION_STATUS.CONCLUIDA);
});

test('getRevisionEstimate soma servicos e pecas com quantidade', () => {
  const estimate = getRevisionEstimate(motos[0].revisoesPlanejadas[0]);

  assert.equal(estimate, 170);
});

test('getExecutionItemStatus entende status explicito de checklist', () => {
  assert.equal(getExecutionItemStatus({ concluido: true }), EXECUTION_ITEM_STATUS.CONCLUIDO);
  assert.equal(getExecutionItemStatus({ emExecucao: true }), EXECUTION_ITEM_STATUS.EM_EXECUCAO);
  assert.equal(getExecutionItemStatus({ statusExecucao: 'Concluído' }), EXECUTION_ITEM_STATUS.CONCLUIDO);
  assert.equal(getExecutionItemStatus({ statusExecucao: 'Em andamento' }), EXECUTION_ITEM_STATUS.EM_EXECUCAO);
  assert.equal(getExecutionItemStatus({ nome: 'Sem progresso informado' }), EXECUTION_ITEM_STATUS.PENDENTE);
});

test('getExecutionProgress calcula progresso de pecas e servicos em execucao', () => {
  const progresso = getExecutionProgress(
    [{ id: 1, concluido: true }, { id: 2, emExecucao: true }],
    [{ id: 3, statusExecucao: 'Pendente' }]
  );

  assert.deepEqual(progresso, {
    total: 3,
    concluidos: 1,
    emExecucao: 1,
    percentual: 33,
  });
});

test('applyRevisionStatusOverride aplica status vindo de agendamentos sem alterar status invalido', () => {
  const revisoes = agruparRevisoesDasMotos(motos, today);
  const revisao = revisoes[1];

  assert.equal(applyRevisionStatusOverride(revisao, 'Em Execução').status, REVISION_STATUS.EM_EXECUCAO);
  assert.equal(applyRevisionStatusOverride(revisao, 'status-invalido').status, revisao.status);
});

test('buildMotoRevisionDetailsPath monta destino unico para detalhes da revisao', () => {
  const path = buildMotoRevisionDetailsPath(
    '/dashboard/cliente/motos/detalhes',
    16,
    99,
    'em_execucao',
    '/dashboard/cliente/agendamentos'
  );

  assert.equal(
    path,
    '/dashboard/cliente/motos/detalhes/16?revisaoMotoId=99&status=em_execucao&returnTo=%2Fdashboard%2Fcliente%2Fagendamentos'
  );
});
