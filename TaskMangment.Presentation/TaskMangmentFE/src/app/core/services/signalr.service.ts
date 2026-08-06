import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { BehaviorSubject } from 'rxjs';

export interface SignalRNotification {
  id?: number;
  message: string;
  link?: string;
  createdAt: Date;
  taskId?: number;
}

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection!: signalR.HubConnection;
  private isStarted = false;
  private isListenerRegistered = false;
  private connectedUserId: number | null = null;

  private notificationSubject = new BehaviorSubject<SignalRNotification | null>(null);

  notification$ = this.notificationSubject.asObservable();

  startConnection(userId: number) {
    if (this.isStarted && this.connectedUserId === userId) {
      return;
    }

    if (this.isStarted || this.hubConnection) {
      this.stop();
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      //.withUrl(`https://taskmangmentapi-bzh2erdwazfea9g8.westeurope-01.azurewebsites.net/notifications?userId=${userId}`)
      .withUrl(`https://localhost:7115/notifications?userId=${userId}`)
      .withAutomaticReconnect()
      .build();

    this.connectedUserId = userId;

    this.hubConnection.start()
      .then(() => {
        console.log('✅ SignalR Connected');
        this.isStarted = true;
        this.registerListener();
      })
      .catch(err => console.error(' SignalR Error', err));
  }

  private registerListener() {
    if (this.isListenerRegistered || !this.hubConnection) return;

    this.hubConnection.on('ReceiveNotification', (data: { id?: number; message: string; taskId?: number }) => {
      const notification: SignalRNotification = {
        id: data.id,
        message: data.message,
        createdAt: new Date(),
        link: '/pages/notifications-list',
        taskId: data.taskId
      };
      console.log('Notification received:', notification.message);
      this.notificationSubject.next(notification);
    });

    this.isListenerRegistered = true;
  }

  clearPendingNotification() {
    this.notificationSubject.next(null);
  }

  stop() {
    this.clearPendingNotification();

    if (this.hubConnection) {
      try {
        this.hubConnection.off('ReceiveNotification');
        void this.hubConnection.stop();
      } catch (err) {
        console.error('SignalR stop error', err);
      }
    }

    this.isStarted = false;
    this.isListenerRegistered = false;
    this.connectedUserId = null;
    this.hubConnection = undefined!;
  }
}
