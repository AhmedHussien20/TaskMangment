import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, HostListener, OnDestroy, OnInit, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { NgbDropdownModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { Subscription } from 'rxjs';
import { AppStateService } from '../../shared/services/app-state.service';
import { TranslationService } from '../../shared/services/translation.service';
import { PlatformSessionService } from '../platform-session.service';

@Component({
  selector: 'app-platform-layout',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, NgbDropdownModule],
  templateUrl: './platform-layout.component.html',
  styleUrls: ['../platform.component.scss']
})
export class PlatformLayoutComponent implements OnInit, OnDestroy {
  sessionService = inject(PlatformSessionService);
  private translation = inject(TranslationService);
  private appState = inject(AppStateService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);
  private langSub?: Subscription;

  countryFlag = 'sa';
  isFullscreen = false;

  ngOnInit(): void {
    document.body.classList.remove('error-1');
    const path = this.router.url.split('?')[0];
    if (path === '/platform' || path === '/platform/') {
      this.router.navigateByUrl(this.sessionService.homePath);
    }
    this.langSub = this.translation.getCurrentLang().subscribe(lang => {
      this.countryFlag = lang === 'ar' ? 'sa' : 'gb';
    });
  }

  ngOnDestroy(): void {
    this.langSub?.unsubscribe();
  }

  changeLanguage(lang: string): void {
    this.translation.setLanguage(lang);
  }

  updateTheme(theme: string): void {
    this.appState.updateState({ theme, menuColor: theme });
    if (theme === 'light') {
      this.appState.updateState({ theme, themeBackground: '', headerColor: 'light', menuColor: 'dark' });
    }
    if (theme === 'dark') {
      this.appState.updateState({ theme, themeBackground: '', headerColor: 'dark', menuColor: 'dark' });
    }
    const html = document.querySelector('html');
    html?.style.removeProperty('--body-bg-rgb');
    html?.style.removeProperty('--body-bg-rgb2');
    html?.style.removeProperty('--light-rgb');
    html?.style.removeProperty('--form-control-bg');
    html?.style.removeProperty('--input-border');
    html?.style.removeProperty('--sidemenu-active-bgcolor');
  }

  toggleFullscreen(): void {
    if (this.isFullscreen) {
      document.exitFullscreen?.();
    } else {
      document.documentElement.requestFullscreen?.();
    }
  }

  @HostListener('document:fullscreenchange')
  onFullscreenChange(): void {
    this.isFullscreen = !!document.fullscreenElement;
    this.cdr.detectChanges();
  }

  toggleSidebar(): void {
    const root = document.documentElement;
    const current = root.getAttribute('data-toggled') || '';
    if (window.innerWidth < 992) {
      root.setAttribute('data-toggled', current === 'open' ? 'close' : 'open');
      return;
    }
    root.setAttribute('data-toggled', current === 'icon-overlay-close' ? '' : 'icon-overlay-close');
  }

  logout(): void {
    this.sessionService.clear();
    this.router.navigateByUrl('/platform/login');
  }
}
