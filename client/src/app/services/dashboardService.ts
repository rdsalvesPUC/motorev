import { t } from '@/app/i18n';
import { DashboardConcessionariaRevisoes } from '@/app/models/DashboardConcessionaria';
import { BASE_URL, apiFetch, handleResponse } from '@/app/services/http';

const DASHBOARD_URL = `${BASE_URL}/Dashboard`;

export const dashboardService = {
  async getConcessionariaRevisoes(data?: string): Promise<DashboardConcessionariaRevisoes> {
    const query = data ? `?data=${encodeURIComponent(data)}` : '';
    const response = await apiFetch(`${DASHBOARD_URL}/concessionaria/revisoes${query}`);
    return handleResponse(response, t('concessionariaDashboard.load.error'));
  },
};
