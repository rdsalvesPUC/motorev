import { BASE_URL, apiFetch } from './http';
import { AlertaResponse, AlertaNaoLidosCountResponse, TipoAlerta } from '../models/Alerta';

export const alertaService = {
  async listar(lido?: boolean, tipo?: TipoAlerta): Promise<AlertaResponse[]> {
    const params = new URLSearchParams();
    if (lido !== undefined) params.append('lido', String(lido));
    if (tipo !== undefined) params.append('tipo', tipo);
    
    const response = await apiFetch(`${BASE_URL}/alertas?${params.toString()}`);
    return await response.json();
  },

  async contarNaoLidos(): Promise<number> {
    const response = await apiFetch(`${BASE_URL}/alertas/nao-lidos/total`);
    const data = await response.json();
    return data.total;
  },

  async marcarComoLido(id: number): Promise<AlertaResponse> {
    const response = await apiFetch(`${BASE_URL}/alertas/${id}/ler`, {
      method: 'PUT'
    });
    return await response.json();
  },

  async marcarTodosComoLidos(): Promise<void> {
    await apiFetch(`${BASE_URL}/alertas/ler-todos`, {
      method: 'PUT'
    });
  }
};
