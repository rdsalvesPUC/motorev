import { apiFetch } from './http';
import { AlertaResponse, AlertaNaoLidosCountResponse, TipoAlerta } from '../models/Alerta';

export const alertaService = {
  async listar(lido?: boolean, tipo?: TipoAlerta): Promise<AlertaResponse[]> {
    const params = new URLSearchParams();
    if (lido !== undefined) params.append('lido', String(lido));
    if (tipo !== undefined) params.append('tipo', tipo);
    
    return await apiFetch<AlertaResponse[]>(`/api/alertas?${params.toString()}`);
  },

  async contarNaoLidos(): Promise<number> {
    const response = await apiFetch<AlertaNaoLidosCountResponse>('/api/alertas/nao-lidos/total');
    return response.total;
  },

  async marcarComoLido(id: number): Promise<AlertaResponse> {
    return await apiFetch<AlertaResponse>(`/api/alertas/${id}/ler`, {
      method: 'PUT'
    });
  },

  async marcarTodosComoLidos(): Promise<void> {
    await apiFetch('/api/alertas/ler-todos', {
      method: 'PUT'
    });
  }
};
