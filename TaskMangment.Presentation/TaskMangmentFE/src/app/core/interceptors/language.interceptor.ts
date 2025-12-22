import { HttpInterceptorFn } from '@angular/common/http';

export const languageInterceptor: HttpInterceptorFn = (req, next) => {
  const lang = localStorage.getItem('lang') || 'ar';

  const langReq = req.clone({
    setHeaders: {
      'Accept-Language': lang
    }
  });

  return next(langReq);
};
