import assert from 'node:assert/strict';
import test from 'node:test';
import {
  AGENDAMENTO_STATUS,
  buildAgendamentoItems,
  getAgendamentoStatus,
} from './AgendamentosCliente.utils.js';

const today = new Date('2026-06-09T12:00:00');

test('getAgendamentoStatus marca revisão dentro da janela sem data como aguardando agendamento', () => {
  const status = getAgendamentoStatus({
    dataPrevista: '2026-06-09T00:00:00',
    status: 'Planejada',
  }, today);

  assert.equal(status, AGENDAMENTO_STATUS.AGUARDANDO_AGENDAMENTO);
});

test('getAgendamentoStatus preserva aguardando confirmação', () => {
  const status = getAgendamentoStatus({
    dataPrevista: '2026-06-09T00:00:00',
    dataAgendamento: '2026-06-10T00:00:00',
    status: 'Aguardando Confirmação',
  }, today);

  assert.equal(status, AGENDAMENTO_STATUS.AGUARDANDO_CONFIRMACAO);
});

test('getAgendamentoStatus marca agendamento passado dentro da tolerância como atrasado', () => {
  const status = getAgendamentoStatus({
    dataPrevista: '2026-06-09T00:00:00',
    dataAgendamento: '2026-06-08T00:00:00',
    status: 'Agendada',
  }, today);

  assert.equal(status, AGENDAMENTO_STATUS.ATRASADA);
});

test('buildAgendamentoItems ignora planejadas fora da janela e concluidas', () => {
  const motos = [
    {
      id: 1,
      revisoesPlanejadas: [
        { id: 10, dataPrevista: '2026-06-09T00:00:00', status: 'Planejada' },
        { id: 11, dataPrevista: '2026-08-09T00:00:00', status: 'Planejada' },
        { id: 12, dataPrevista: '2026-06-09T00:00:00', status: 'Concluida' },
      ],
    },
  ];

  const items = buildAgendamentoItems(motos, today);

  assert.equal(items.length, 1);
  assert.equal(items[0].revisao.id, 10);
  assert.equal(items[0].status, AGENDAMENTO_STATUS.AGUARDANDO_AGENDAMENTO);
});
