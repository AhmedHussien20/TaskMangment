import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {

  private hubConnection!: signalR.HubConnection;

  startConnection() {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('https://localhost:7115/notifications', {
        withCredentials: true
      })
      .withAutomaticReconnect()
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.hubConnection.start()
      .then(() => console.log('SignalR Connected'))
      .catch(err => console.log('Error while connecting SignalR:', err));
  }

  listen(eventName: string, callback: (...args: any[]) => void) {
    this.hubConnection.on(eventName, callback);
  }

  send(eventName: string, ...args: any[]) {
    this.hubConnection.invoke(eventName, ...args)
      .catch(err => console.log('Error invoking hub method:', err));
  }
}
