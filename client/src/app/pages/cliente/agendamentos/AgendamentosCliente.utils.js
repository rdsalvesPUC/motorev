export const AGENDAMENTO_STATUS = {
  PLANEJADA: 'planejada',
  AGUARDANDO_AGENDAMENTO: 'aguardando_agendamento',
  AGUARDANDO_CONFIRMACAO: 'aguardando_confirmacao',
  AGENDADA: 'agendada',
  EM_EXECUCAO: 'em_execucao',
  CONCLUIDA: 'concluida',
  ATRASADA: 'atrasada',
  PERDIDA: 'perdida',
};

export const AGENDAMENTO_VISIBLE_STATUSES = new Set([
  AGENDAMENTO_STATUS.AGUARDANDO_AGENDAMENTO,
  AGENDAMENTO_STATUS.AGUARDANDO_CONFIRMACAO,
  AGENDAMENTO_STATUS.AGENDADA,
  AGENDAMENTO_STATUS.EM_EXECUCAO,
  AGENDAMENTO_STATUS.ATRASADA,
  AGENDAMENTO_STATUS.PERDIDA,
]);

export function normalizeStatus(status) {
  return String(status ?? '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/\s+/g, '_');
}

export function parseLocalDate(date) {
  if (!date) return new Date(Number.NaN);
  const value = String(date).substring(0, 10);
  return new Date(`${value}T00:00:00`);
}

export function addDays(date, days) {
  const next = new Date(date);
  next.setDate(next.getDate() + days);
  next.setHours(0, 0, 0, 0);
  return next;
}

export function getAgendamentoWindow(revisao) {
  const dataIdeal = parseLocalDate(revisao?.dataPrevista);
  return {
    dataIdeal,
    dataMinima: addDays(dataIdeal, -15),
    dataLimite: addDays(dataIdeal, 15),
  };
}

export function getAgendamentoStatus(revisao, today = new Date()) {
  const status = normalizeStatus(revisao?.status);
  const referenceDate = new Date(today);
  referenceDate.setHours(0, 0, 0, 0);

  if (status === AGENDAMENTO_STATUS.CONCLUIDA) return AGENDAMENTO_STATUS.CONCLUIDA;
  if (status === AGENDAMENTO_STATUS.EM_EXECUCAO) return AGENDAMENTO_STATUS.EM_EXECUCAO;
  if (status === AGENDAMENTO_STATUS.AGUARDANDO_CONFIRMACAO) return AGENDAMENTO_STATUS.AGUARDANDO_CONFIRMACAO;

  const { dataMinima, dataLimite } = getAgendamentoWindow(revisao);
  if (referenceDate > dataLimite) return AGENDAMENTO_STATUS.PERDIDA;
  if (referenceDate < dataMinima) return AGENDAMENTO_STATUS.PLANEJADA;

  const dataAgendamento = revisao?.dataAgendamento ? parseLocalDate(revisao.dataAgendamento) : null;
  if (dataAgendamento) {
    return referenceDate > dataAgendamento
      ? AGENDAMENTO_STATUS.ATRASADA
      : AGENDAMENTO_STATUS.AGENDADA;
  }

  if (status === AGENDAMENTO_STATUS.AGENDADA) return AGENDAMENTO_STATUS.AGENDADA;
  if (status === AGENDAMENTO_STATUS.PERDIDA) return AGENDAMENTO_STATUS.PERDIDA;

  return AGENDAMENTO_STATUS.AGUARDANDO_AGENDAMENTO;
}

export function getPrazoTexto(item, today = new Date()) {
  const referenceDate = new Date(today);
  referenceDate.setHours(0, 0, 0, 0);
  const diffLimite = Math.ceil((item.dataLimite.getTime() - referenceDate.getTime()) / 86400000);
  const diffAgendamento = item.revisao?.dataAgendamento
    ? Math.ceil((parseLocalDate(item.revisao.dataAgendamento).getTime() - referenceDate.getTime()) / 86400000)
    : null;

  if (item.status === AGENDAMENTO_STATUS.EM_EXECUCAO) return 'Revisão em execução';
  if (item.status === AGENDAMENTO_STATUS.AGUARDANDO_CONFIRMACAO) return 'Aguardando confirmação da concessionária';
  if (item.status === AGENDAMENTO_STATUS.PERDIDA) return `Prazo encerrado há ${Math.abs(diffLimite)} dia(s)`;
  if (item.status === AGENDAMENTO_STATUS.ATRASADA) return `Agendamento atrasado. Reagende em até ${diffLimite} dia(s)`;
  if (item.status === AGENDAMENTO_STATUS.AGENDADA && diffAgendamento !== null) {
    if (diffAgendamento === 0) return 'Agendada para hoje';
    if (diffAgendamento > 0) return `Agendada em ${diffAgendamento} dia(s)`;
    return `Agendada há ${Math.abs(diffAgendamento)} dia(s)`;
  }
  if (diffLimite === 0) return 'Último dia para agendar';
  return `Faltam ${diffLimite} dia(s) para o limite`;
}

export function buildAgendamentoItems(motos, today = new Date()) {
  return (motos ?? [])
    .flatMap((moto) =>
      (moto.revisoesPlanejadas ?? []).map((revisao) => {
        const status = getAgendamentoStatus(revisao, today);
        const window = getAgendamentoWindow(revisao);
        const item = {
          key: `${moto.id}-${revisao.id}`,
          moto,
          revisao,
          status,
          ...window,
        };

        return {
          ...item,
          prazoTexto: getPrazoTexto(item, today),
        };
      })
    )
    .filter((item) => AGENDAMENTO_VISIBLE_STATUSES.has(item.status))
    .sort((a, b) => {
      const priority = {
        [AGENDAMENTO_STATUS.EM_EXECUCAO]: 0,
        [AGENDAMENTO_STATUS.ATRASADA]: 1,
        [AGENDAMENTO_STATUS.AGUARDANDO_CONFIRMACAO]: 2,
        [AGENDAMENTO_STATUS.AGENDADA]: 3,
        [AGENDAMENTO_STATUS.AGUARDANDO_AGENDAMENTO]: 4,
        [AGENDAMENTO_STATUS.PERDIDA]: 5,
      };
      const statusOrder = priority[a.status] - priority[b.status];
      if (statusOrder !== 0) return statusOrder;
      return a.dataIdeal.getTime() - b.dataIdeal.getTime();
    });
}
