import { BASE_URL, apiFetch, handleResponse } from '@/app/services/http';
import { t } from '@/app/i18n';
import { AgendamentoCliente } from '@/app/models/AgendamentoCliente';

const AGENDAMENTO_URL = `${BASE_URL}/Agendamento`;

export const agendamentoService = {
  async getCliente(): Promise<AgendamentoCliente[]> {
    const response = await apiFetch(`${AGENDAMENTO_URL}/cliente`);
    return handleResponse(response, t('clienteAgendamentos.load.error'));
  },
};
