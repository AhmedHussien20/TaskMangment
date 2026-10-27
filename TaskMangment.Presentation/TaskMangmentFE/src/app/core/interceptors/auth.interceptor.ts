import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { tap } from 'rxjs/operators';

export const AuthInterceptor: HttpInterceptorFn = (req, next) => {

  const toastr = inject(ToastrService);

  console.log('%c[INTERCEPTOR] Request intercepted', 'color: green', req.url);

  const isPlatformRequest = req.url.includes('/platform');
  const token = isPlatformRequest
    ? localStorage.getItem('platformToken')
    : localStorage.getItem('authToken');

  if (token && !req.url.includes('/platform/login')) {
    req = req.clone({
      setHeaders: { Authorization: `Bearer ${token}` }
    });
  }

  return next(req).pipe(
    tap({
      error: (err) => {
        const isCompanyLoginRequest =
          /\/auth\/(login|forgot-password|verify-reset-code|reset-password)/i.test(req.url);
        const isPlatformLoginRequest = /\/platform\/login/i.test(req.url);

        // Let the login page show the error; don't reload while already on login.
        if (isCompanyLoginRequest || isPlatformLoginRequest) {
          return;
        }

        const isInactiveAccount =
          err.status === 403 && err.error?.errorCode === 'EMPLOYEE_INACTIVE';

        if (err.status === 401 || isInactiveAccount) {
          if (isPlatformRequest) {
            localStorage.removeItem('platformToken');
            localStorage.removeItem('platformUser');
            toastr.error(getMessage('SESSION_EXPIRED'), getMessage('UNAUTHORIZED'), {
              timeOut: 3000,
              positionClass: 'toast-top-right'
            });
            window.location.href = '/platform/login';
            return;
          }

          localStorage.removeItem('authToken');
          localStorage.removeItem('userData');
          localStorage.removeItem('refreshToken');

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
