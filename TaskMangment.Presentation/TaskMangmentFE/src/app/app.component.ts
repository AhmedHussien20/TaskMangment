import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { AppStateService } from './shared/services/app-state.service';
import { Store } from '@ngrx/store';
import * as NavActions from './store/nav/nav.actions';
import { TranslationService } from './shared/services/translation.service';
import { SignalRService } from './core/services/signalr.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'Task-Portal';
  constructor(private appState: AppStateService, private store: Store, private translationService: TranslationService, private signalR: SignalRService) { }

  ngOnInit(): void {
    this.store.dispatch(NavActions.initializeMenu());  // ✅ Move dispatch here
    this.appState.updateState();
    this.signalR.startConnection();

    this.signalR.listen("ReceiveMessage", (user, message) => {
      console.log("Message received:", user, message);
    });
  }
}
