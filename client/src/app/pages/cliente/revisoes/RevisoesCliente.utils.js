export const REVISION_STATUS = {
  CONCLUIDA: 'concluida',
  EM_EXECUCAO: 'em_execucao',
  AGENDADA: 'agendada',
  ATRASADA: 'atrasada',
  PLANEJADA: 'planejada',
};

const STATUS_PRIORITY = {
  [REVISION_STATUS.ATRASADA]: 0,
  [REVISION_STATUS.EM_EXECUCAO]: 1,
  [REVISION_STATUS.AGENDADA]: 2,
  [REVISION_STATUS.PLANEJADA]: 3,
  [REVISION_STATUS.CONCLUIDA]: 4,
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
  if (status.includes('agendada')) return REVISION_STATUS.AGENDADA;
  if (status.includes('atrasada')) return REVISION_STATUS.ATRASADA;

  const referenceDate = new Date(today);
  referenceDate.setHours(0, 0, 0, 0);
  return parseLocalDate(revisao?.dataPrevista) < referenceDate
    ? REVISION_STATUS.ATRASADA
    : REVISION_STATUS.PLANEJADA;
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

export function agruparRevisoesDasMotos(motos, today = new Date()) {
  return (motos ?? []).flatMap((moto) =>
    (moto.revisoesPlanejadas ?? []).map((revisao) => {
      const status = getRevisionStatus(revisao, today);

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
        revisao,
        status,
        dataPrevista: revisao.dataPrevista,
        ordem: revisao.ordem,
        quilometragem: revisao.quilometragem,
        diasAteRevisao: getDaysUntilRevision(revisao, today),
        totalPecas: getRevisionPartsEstimate(revisao),
        totalServicos: getRevisionServicesEstimate(revisao),
        totalEstimado: getRevisionEstimate(revisao),
        totalTempo: getRevisionTime(revisao),
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
    if (item.status === REVISION_STATUS.PLANEJADA) resumo.pendentes += 1;
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
