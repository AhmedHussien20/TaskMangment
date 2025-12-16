import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { BehaviorSubject } from 'rxjs';
@Injectable({ providedIn: 'root' })
export class SignalRService {

  private hubConnection!: signalR.HubConnection;
  private isStarted = false;
  private isListenerRegistered = false;

  private notificationSubject = new BehaviorSubject<string | null>(null);
  notification$ = this.notificationSubject.asObservable();
constructor(  
  private toastr: ToastrService
) {}
  startConnection(userId: number,) {

    if (this.isStarted) return;   

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`https://localhost:7115/notifications?userId=${userId}`)
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start()
      .then(() => {
        console.log('✅ SignalR Connected');
        this.isStarted = true;
        this.registerListener();
      })
      .catch(err => console.error(' SignalR Error', err));
  }

  private registerListener() {

    if (this.isListenerRegistered) return;  

    this.hubConnection.on('ReceiveNotification', (message: string) => {
      console.log('Notification received:', message);
      this.toastr.info(message, 'Notification');
      this.notificationSubject.next(message);
    });

    this.isListenerRegistered = true;
  }

  stop() {
    this.hubConnection?.stop();
    this.isStarted = false;
    this.isListenerRegistered = false;
  }
}
