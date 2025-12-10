import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, HttpClient, withInterceptors } from '@angular/common/http';
import { AppComponent } from './app/app.component';
import { register as registerSwiperElements } from 'swiper/element';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';

import { navReducer } from './app/store/nav/nav.reducer';
import { NavEffects } from './app/store/nav/nav.effects';
import { authReducer } from './app/store/auth/auth.reducer';
import { AuthEffects } from './app/store/auth/auth.effects';

import { provideRouter } from '@angular/router';
import { App_Route } from './app/app.routes';

import { provideAuth, getAuth } from '@angular/fire/auth';
import { provideToastr } from 'ngx-toastr';
import { provideAnimations } from '@angular/platform-browser/animations';

import { CommonModule } from '@angular/common';

import {
  TranslateModule,
  TranslateService,
  TranslateStore,
  TranslateLoader,
  TranslateCompiler,
  TranslateFakeCompiler,
  TranslateParser,
  TranslateDefaultParser,
  MissingTranslationHandler,
  FakeMissingTranslationHandler,
  USE_DEFAULT_LANG,
  USE_EXTEND,
  DEFAULT_LANGUAGE,
  ISOLATE_TRANSLATE_SERVICE
} from '@ngx-translate/core';

import { TranslateHttpLoader } from '@ngx-translate/http-loader';

import { ApiService } from 'app/core/services/api.service';
import { BranchRepository } from 'app/core/repositories/branch.repository'; 
import { importProvidersFrom } from '@angular/core';
import { jwtInterceptor } from 'app/core/auth/jwt.interceptor';

/* ---------------------------
   Translate Loader Factory
---------------------------- */
export function HttpLoaderFactory(http: HttpClient) {
  return new TranslateHttpLoader(http, './assets/i18n/', '.json');
}

registerSwiperElements();

/* ---------------------------
         BOOTSTRAP APP
---------------------------- */
bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(App_Route),

    provideHttpClient(
      withInterceptors([jwtInterceptor])
    ),
    /* ---------------------------
           GLOBAL STATE
    ---------------------------- */
    provideStore({
      nav: navReducer,
      auth: authReducer,
    }),

    provideEffects([
      NavEffects,
      AuthEffects,
    ]),

    /* ---------------------------
           CORE PROVIDERS
    ---------------------------- */
    provideHttpClient(),
    provideAuth(() => getAuth()),
    provideToastr(),
    provideAnimations(),
    CommonModule,

    
    /* ---------------------------
            TRANSLATION
    ---------------------------- */
    TranslateService,
    TranslateStore,

    {
      provide: TranslateLoader,
      useFactory: HttpLoaderFactory,
      deps: [HttpClient],
    },

    { provide: TranslateCompiler, useClass: TranslateFakeCompiler },
    { provide: TranslateParser, useClass: TranslateDefaultParser },
    {
      provide: MissingTranslationHandler,
      useClass: FakeMissingTranslationHandler,
    },

    /* IMPORTANT: language settings */
    { provide: USE_DEFAULT_LANG, useValue: true },
    { provide: DEFAULT_LANGUAGE, useValue: 'ar' },

    /* Fix: Isolated translate must be false to allow global translation */
    { provide: ISOLATE_TRANSLATE_SERVICE, useValue: false },

    /* Extend translation files instead of replacing */
    { provide: USE_EXTEND, useValue: true },

  importProvidersFrom(
  TranslateModule.forRoot({
    defaultLanguage: 'ar',
    loader: {
      provide: TranslateLoader,
      useFactory: HttpLoaderFactory,
      deps: [HttpClient],
    },
  })
),

  ],
}).catch(err => console.error(err));
