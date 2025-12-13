import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject } from 'rxjs';
import { AppStateService } from './app-state.service';
import { AppState } from 'app/store/app.state';
import { Store } from '@ngrx/store';
import { updateMenuTranslations } from 'app/store/nav/nav.actions';

@Injectable({
  providedIn: 'root'
})
export class TranslationService {
  private currentLang = new BehaviorSubject<string>('ar');
  private readonly storageKey = 'app-language';

  constructor(
    private translate: TranslateService,
    private appStateService: AppStateService,
    private store: Store<AppState>
  ) {
    const savedLang = localStorage.getItem(this.storageKey);
    this.translate.setDefaultLang('ar');
    this.setLanguage(savedLang || 'ar');
  }

  setLanguage(lang: string) {
    this.translate.use(lang).subscribe(() => {
      localStorage.setItem(this.storageKey, lang);
      this.currentLang.next(lang);
  
      if(lang === 'ar') {
        this.appStateService.updateState({ direction: 'rtl' });
      } else {
        this.appStateService.updateState({ direction: 'ltr' });
      }
  
      // ✅ Ensure translations are fully loaded before dispatching
      this.store.dispatch(updateMenuTranslations());
    });
  }  

  getCurrentLang() {
    return this.currentLang.asObservable();
  }
}
