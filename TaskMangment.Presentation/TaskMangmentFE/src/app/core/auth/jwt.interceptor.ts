
import { HttpInterceptorFn } from '@angular/common/http'; 
 
export const jwtInterceptor : HttpInterceptorFn = (req,next) =>{
    const isPlatformRequest = req.url.includes('/platform');
    const token = isPlatformRequest
      ? localStorage.getItem('platformToken')
      : localStorage.getItem('authToken');
    if (token && !req.url.includes('/platform/login')) {
      req = req.clone({
        setHeaders: { Authorization: `Bearer ${token}` }
      });
    }
    return next(req);
  
}
