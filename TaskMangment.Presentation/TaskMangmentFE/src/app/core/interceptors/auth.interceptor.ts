import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';
import { tap } from 'rxjs/operators';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {

  const router = inject(Router);
  const toastr = inject(ToastrService);

  console.log('%c[INTERCEPTOR] Request intercepted', 'color: green', req.url);

  const token = localStorage.getItem('authToken');

  if (token) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(req).pipe(
    tap({
      error: (err) => {
        if (err.status === 401) {

          // Clear token
          localStorage.clear();

          // Show toast
          toastr.error(
            getMessage('SESSION_EXPIRED'),
            getMessage('UNAUTHORIZED'),
            {
              timeOut: 3000,
              positionClass: 'toast-top-right'
            }
          );

          // Redirect to login
          window.location.href = '/auth/login';
        }
      }
    })
  );
};


function getMessage(key: string): string {
  const lang = localStorage.getItem('lang') || 'en';

  const messages: any = {
    SESSION_EXPIRED: {
      en: 'Your session has expired. Please login again.',
      ar: 'انتهت صلاحية الجلسة. يرجى تسجيل الدخول مرة أخرى.'
    },
    UNAUTHORIZED: {
      en: 'Unauthorized',
      ar: 'غير مصرح'
    }
  };

  return messages[key][lang] || messages[key].en;
}
