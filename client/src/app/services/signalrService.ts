import * as signalR from '@microsoft/signalr';
import { tokenManager } from './authService';
import { Alerta } from '../models/Alerta';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private onAlertaRecebidoCallback: ((alerta: Alerta) => void) | null = null;

  public async startConnection() {
    if (this.connection) return;

    const baseUrl = import.meta.env.VITE_API_URL || 'http://localhost:5258';
    
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(`${baseUrl}/hubs/notifications`, {
        accessTokenFactory: () => tokenManager.getAccessToken() || '',
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReceberAlerta', (alerta: Alerta) => {
      if (this.onAlertaRecebidoCallback) {
        this.onAlertaRecebidoCallback(alerta);
      }
    });

    try {
      await this.connection.start();
      console.log('SignalR Connected.');
    } catch (err) {
      console.error('SignalR Connection Error: ', err);
      // Tenta reconectar após 5 segundos se falhar inicialmente
      setTimeout(() => this.startConnection(), 5000);
    }
  }

  public stopConnection() {
    if (this.connection) {
      this.connection.stop();
      this.connection = null;
    }
  }

  public onAlertaRecebido(callback: (alerta: Alerta) => void) {
    this.onAlertaRecebidoCallback = callback;
  }
}

export const signalRService = new SignalRService();
