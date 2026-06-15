import { BASE_URL, apiFetch, handleResponse } from '@/app/services/http';
import { t } from '@/app/i18n';
import { AgendamentoCliente } from '@/app/models/AgendamentoCliente';

const AGENDAMENTO_URL = `${BASE_URL}/Agendamento`;

export const agendamentoService = {
  async getCliente(): Promise<AgendamentoCliente[]> {
    const response = await apiFetch(`${AGENDAMENTO_URL}/cliente`);
    return handleResponse(response, t('clienteAgendamentos.load.error'));
  },

  async cancelarCliente(agendamentoId: number): Promise<void> {
    const response = await apiFetch(`${AGENDAMENTO_URL}/cliente/${agendamentoId}/cancelar`, {
      method: 'PATCH',
    });
    await handleResponse(response, t('clienteAgendamentos.actions.cancel.error'));
  },

  async remarcarCliente(agendamentoId: number, novaData: string): Promise<void> {
    const response = await apiFetch(`${AGENDAMENTO_URL}/cliente/${agendamentoId}/remarcar`, {
      method: 'POST',
      body: JSON.stringify({ novaData }),
    });
    await handleResponse(response, t('clienteAgendamentos.actions.reschedule.error'));
  },

  async agendarCliente(revisaoMotoId: number, lojaId: number, dataAgendada: string): Promise<void> {
    const response = await apiFetch(`${AGENDAMENTO_URL}/cliente/revisoes/${revisaoMotoId}/agendar`, {
      method: 'POST',
      body: JSON.stringify({ lojaId, dataAgendada }),
    });
    await handleResponse(response, t('clienteAgendamentos.actions.schedule.error'));
  },
};
