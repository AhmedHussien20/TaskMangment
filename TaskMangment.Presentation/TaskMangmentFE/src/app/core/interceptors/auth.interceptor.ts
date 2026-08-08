import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { tap } from 'rxjs/operators';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {

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
        const isAuthRequest = /\/auth\/(login|forgot-password|verify-reset-code|reset-password)/i.test(req.url);

        const isInactiveAccount =
          err.status === 403 && err.error?.errorCode === 'EMPLOYEE_INACTIVE';

        // Let the login page show the error; don't reload while already on login.
        if (isAuthRequest) {
          return;
        }

        if (err.status === 401 || isInactiveAccount) {

          // Clear token
          localStorage.clear();

          // Show toast
          toastr.error(
            isInactiveAccount
              ? getMessage('ACCOUNT_INACTIVE')
              : getMessage('SESSION_EXPIRED'),
            isInactiveAccount
              ? getMessage('ACCOUNT_DISABLED')
              : getMessage('UNAUTHORIZED'),
            {
              timeOut: 3000,
              positionClass: 'toast-top-right'
            }
          );

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
    },
    ACCOUNT_INACTIVE: {
      en: 'Your account is inactive. Please contact the administrator.',
      ar: 'حسابك غير نشط. يرجى التواصل مع المسؤول.'
    },
    ACCOUNT_DISABLED: {
      en: 'Account disabled',
      ar: 'الحساب معطل'
    }
  };

  return messages[key][lang] || messages[key].en;
}
