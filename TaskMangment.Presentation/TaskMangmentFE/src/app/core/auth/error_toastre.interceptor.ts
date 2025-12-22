import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { ToastrService } from 'ngx-toastr';
import { catchError, tap } from 'rxjs/operators';
import { throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const toastr = inject(ToastrService);

  return next(req).pipe(
    tap({
      next: (event: any) => {
        if (event?.body?.success === false) {
          const message = event.body?.message || 'حدث خطأ.';
          toastr.error(message, 'خطأ', {
            timeOut: 3000,
            positionClass: 'toast-top-right'
          });
        }
      }
    }),
    catchError((err: any) => {
      const message = err?.error?.message || 'حدث خطأ. يرجى المحاولة مرة أخرى.';
      toastr.error(message, 'خطأ', {
        timeOut: 3000,
        positionClass: 'toast-top-right'
      });
      return throwError(() => err);
    })
  );
};
