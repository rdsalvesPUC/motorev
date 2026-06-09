import * as signalR from '@microsoft/signalr';
import { BASE_URL } from '@/app/services/http';
import { tokenManager } from './tokenManager';
import { Alerta } from '../models/Alerta';

class SignalRService {
  private connection: signalR.HubConnection | null = null;
  private onAlertaRecebidoCallback: ((alerta: Alerta) => void) | null = null;

  public async startConnection() {
    if (this.connection) return;

    const hubUrl = BASE_URL.replace('/api', '') + '/hubs/notifications';
    
    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => tokenManager.getAccessToken() || '',
        skipNegotiation: false,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect()
      .build();

    this.connection.on('ReceberAlerta', (alerta: Alerta) => {
      console.log('[SignalR] Alerta recebido:', alerta);
      if (this.onAlertaRecebidoCallback) {
        this.onAlertaRecebidoCallback(alerta);
      }
    });

    try {
      await this.connection.start();
      console.log('[SignalR] Conexão estabelecida.');
    } catch (err) {
      console.error('[SignalR] Erro ao iniciar conexão:', err);
      // O withAutomaticReconnect() ajuda, mas se falhar o start inicial 
      // podemos tentar novamente manualmente
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
