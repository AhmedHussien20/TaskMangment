import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { appConfig } from './app/app.config';
import { importProvidersFrom, LOCALE_ID } from '@angular/core';
import { registerLocaleData } from '@angular/common';
import localeAr from '@angular/common/locales/ar';
import { MAT_DATE_LOCALE, MatNativeDateModule } from '@angular/material/core';

registerLocaleData(localeAr);

bootstrapApplication(AppComponent, {
  ...appConfig,
  providers: [
    ...(appConfig.providers || []),
    { provide: LOCALE_ID, useValue: 'ar-EG' },
    { provide: MAT_DATE_LOCALE, useValue: 'ar-EG' },
      importProvidersFrom(MatNativeDateModule)

  ]
}).catch(err => console.error(err));