import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterOutlet } from '@angular/router';
import { AppStateService } from './shared/services/app-state.service';
import { Store } from '@ngrx/store';
import * as NavActions from './store/nav/nav.actions';
import { TranslationService } from './shared/services/translation.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'B2B-Portal';
  constructor(private appState: AppStateService, private store: Store, private translationService: TranslationService) {}

  ngOnInit(): void {
    console.log('🔥 Dispatching initializeMenu');
    this.store.dispatch(NavActions.initializeMenu());  // ✅ Move dispatch here
    this.appState.updateState();
  }
}
