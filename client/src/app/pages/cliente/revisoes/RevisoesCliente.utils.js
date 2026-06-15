export const REVISION_STATUS = {
  CONCLUIDA: 'concluida',
  EM_EXECUCAO: 'em_execucao',
  AGUARDANDO_CONFIRMACAO: 'aguardando_confirmacao',
  AGENDADA: 'agendada',
  ATRASADA: 'atrasada',
  AGUARDANDO_AGENDAMENTO: 'aguardando_agendamento',
  PLANEJADA: 'planejada',
};

export const EXECUTION_ITEM_STATUS = {
  CONCLUIDO: 'concluido',
  EM_EXECUCAO: 'em_execucao',
  PENDENTE: 'pendente',
};

const STATUS_PRIORITY = {
  [REVISION_STATUS.ATRASADA]: 0,
  [REVISION_STATUS.EM_EXECUCAO]: 1,
  [REVISION_STATUS.AGUARDANDO_CONFIRMACAO]: 2,
  [REVISION_STATUS.AGENDADA]: 3,
  [REVISION_STATUS.AGUARDANDO_AGENDAMENTO]: 4,
  [REVISION_STATUS.PLANEJADA]: 5,
  [REVISION_STATUS.CONCLUIDA]: 6,
};

export function normalizeStatus(status) {
  return String(status ?? '')
    .normalize('NFD')
    .replace(/[\u0300-\u036f]/g, '')
    .toLowerCase()
    .replace(/\s+/g, '_');
}

export function parseLocalDate(date) {
  if (!date) return new Date(Number.NaN);
  return new Date(`${String(date).substring(0, 10)}T00:00:00`);
}

export function getRevisionStatus(revisao, today = new Date()) {
  const status = normalizeStatus(revisao?.status);
  if (status.includes('concluida')) return REVISION_STATUS.CONCLUIDA;
  if (status.includes('execucao')) return REVISION_STATUS.EM_EXECUCAO;
  if (status.includes('aguardando_confirmacao')) return REVISION_STATUS.AGUARDANDO_CONFIRMACAO;
  if (status.includes('aguardando_agendamento')) return REVISION_STATUS.AGUARDANDO_AGENDAMENTO;
  if (status.includes('agendada')) return REVISION_STATUS.AGENDADA;
  if (status.includes('atrasada')) return REVISION_STATUS.ATRASADA;

  const referenceDate = new Date(today);
  referenceDate.setHours(0, 0, 0, 0);
  const dataPrevista = parseLocalDate(revisao?.dataPrevista);
  const dataMinima = new Date(dataPrevista);
  dataMinima.setDate(dataMinima.getDate() - 15);
  const dataLimite = new Date(dataPrevista);
  dataLimite.setDate(dataLimite.getDate() + 15);

  if (referenceDate < dataMinima) return REVISION_STATUS.PLANEJADA;
  if (referenceDate <= dataLimite) return REVISION_STATUS.AGUARDANDO_AGENDAMENTO;
  return REVISION_STATUS.ATRASADA;
}

export function getRevisionPartsEstimate(revisao) {
  return (revisao?.pecas ?? []).reduce(
    (total, peca) => total + (Number(peca.preco || 0) * Number(peca.quantidade || 0)),
    0
  );
}

export function getRevisionServicesEstimate(revisao) {
  return (revisao?.servicos ?? []).reduce(
    (total, servico) => total + Number(servico.custo || 0),
    0
  );
}

export function getRevisionEstimate(revisao) {
  return getRevisionPartsEstimate(revisao) + getRevisionServicesEstimate(revisao);
}

export function getRevisionTime(revisao) {
  return (revisao?.servicos ?? []).reduce(
    (total, servico) => total + Number(servico.tempoEstimado || 0),
    0
  );
}

export function getDaysUntilRevision(revisao, today = new Date()) {
  const referenceDate = new Date(today);
  referenceDate.setHours(0, 0, 0, 0);
  return Math.ceil((parseLocalDate(revisao?.dataPrevista).getTime() - referenceDate.getTime()) / 86400000);
}

export function agruparRevisoesDasMotos(motos, today = new Date(), agendamentos = []) {
  const agendamentoPorRevisao = new Map(
    (agendamentos ?? []).map((agendamento) => [agendamento.revisaoMotoId, agendamento])
  );

  return (motos ?? []).flatMap((moto) =>
    (moto.revisoesPlanejadas ?? []).map((revisao) => {
      const agendamento = agendamentoPorRevisao.get(revisao.id);
      const revisaoComStatus = agendamento
        ? { ...revisao, status: agendamento.status }
        : revisao;
      const status = getRevisionStatus(revisaoComStatus, today);

      return {
        key: `${moto.id}-${revisao.id}`,
        motoId: moto.id,
        moto: {
          id: moto.id,
          placa: moto.placa,
          marca: moto.marca,
          nomeModelo: moto.nomeModelo,
          ano: moto.ano,
          cor: moto.cor,
          linha: moto.linha,
          kilometragemAtual: moto.kilometragemAtual,
          foto: moto.foto,
        },
        revisao: revisaoComStatus,
        status,
        dataPrevista: revisaoComStatus.dataPrevista,
        ordem: revisaoComStatus.ordem,
        quilometragem: revisaoComStatus.quilometragem,
        diasAteRevisao: getDaysUntilRevision(revisao, today),
        totalPecas: getRevisionPartsEstimate(revisaoComStatus),
        totalServicos: getRevisionServicesEstimate(revisaoComStatus),
        totalEstimado: getRevisionEstimate(revisaoComStatus),
        totalTempo: getRevisionTime(revisaoComStatus),
      };
    })
  );
}

export function calcularResumoRevisoes(revisoes) {
  const resumo = {
    total: revisoes.length,
    concluidas: 0,
    pendentes: 0,
    agendadas: 0,
    emExecucao: 0,
    atrasadas: 0,
  };

  for (const item of revisoes) {
    if (item.status === REVISION_STATUS.CONCLUIDA) resumo.concluidas += 1;
    if (item.status === REVISION_STATUS.PLANEJADA || item.status === REVISION_STATUS.AGUARDANDO_AGENDAMENTO) {
      resumo.pendentes += 1;
    }
    if (item.status === REVISION_STATUS.AGENDADA) resumo.agendadas += 1;
    if (item.status === REVISION_STATUS.EM_EXECUCAO) resumo.emExecucao += 1;
    if (item.status === REVISION_STATUS.ATRASADA) resumo.atrasadas += 1;
  }

  return resumo;
}

export function filtrarRevisoes(revisoes, filtros = {}) {
  const motoFiltro = filtros.motoId ?? 'todas';
  const statusFiltro = filtros.status ?? 'todos';
  const termo = String(filtros.busca ?? '').trim().toLowerCase();
  const digits = termo.replace(/\D/g, '');

  return revisoes.filter((item) => {
    const matchMoto = motoFiltro === 'todas' || Number(item.motoId) === Number(motoFiltro);
    const matchStatus = statusFiltro === 'todos' || item.status === statusFiltro;
    const searchable = [
      item.revisao?.nome,
      item.moto?.placa,
      item.moto?.marca,
      item.moto?.nomeModelo,
      item.moto?.linha,
      item.moto?.cor,
    ]
      .filter(Boolean)
      .join(' ')
      .toLowerCase();
    const placaDigits = String(item.moto?.placa ?? '').replace(/\D/g, '');
    const matchBusca = !termo || searchable.includes(termo) || (digits.length > 0 && placaDigits.includes(digits));

    return matchMoto && matchStatus && matchBusca;
  });
}

export function ordenarRevisoes(revisoes) {
  return revisoes.slice().sort((a, b) => {
    const statusOrder = STATUS_PRIORITY[a.status] - STATUS_PRIORITY[b.status];
    if (statusOrder !== 0) return statusOrder;

    const dateOrder = parseLocalDate(a.dataPrevista).getTime() - parseLocalDate(b.dataPrevista).getTime();
    if (dateOrder !== 0) return dateOrder;

    return a.ordem - b.ordem;
  });
}

export function getExecutionItemStatus(item) {
  if (item?.concluido === true) return EXECUTION_ITEM_STATUS.CONCLUIDO;
  if (item?.emExecucao === true) return EXECUTION_ITEM_STATUS.EM_EXECUCAO;

  const status = normalizeStatus(item?.statusExecucao ?? item?.statusItem ?? item?.status);
  if (status.includes('concluid')) return EXECUTION_ITEM_STATUS.CONCLUIDO;
  if (status.includes('execucao') || status.includes('andamento')) return EXECUTION_ITEM_STATUS.EM_EXECUCAO;

  return EXECUTION_ITEM_STATUS.PENDENTE;
}

export function getExecutionProgress(servicos = [], pecas = []) {
  const items = [...servicos, ...pecas];
  const total = items.length;
  const concluidos = items.filter((item) => getExecutionItemStatus(item) === EXECUTION_ITEM_STATUS.CONCLUIDO).length;
  const emExecucao = items.filter((item) => getExecutionItemStatus(item) === EXECUTION_ITEM_STATUS.EM_EXECUCAO).length;

  return {
    total,
    concluidos,
    emExecucao,
    percentual: total > 0 ? Math.round((concluidos / total) * 100) : 0,
  };
}

export function applyRevisionStatusOverride(revisao, status) {
  if (!revisao || !status) return revisao;

  const normalizedStatus = normalizeStatus(status);
  if (!Object.values(REVISION_STATUS).includes(normalizedStatus)) return revisao;

  return {
    ...revisao,
    status: normalizedStatus,
  };
}

export function buildMotoRevisionDetailsPath(basePath, motoId, revisaoMotoId, status, returnTo) {
  const params = new URLSearchParams();
  if (revisaoMotoId) params.set('revisaoMotoId', String(revisaoMotoId));
  if (status) params.set('status', String(status));
  if (returnTo) params.set('returnTo', String(returnTo));

  const query = params.toString();
  return `${basePath}/${motoId}${query ? `?${query}` : ''}`;
}
