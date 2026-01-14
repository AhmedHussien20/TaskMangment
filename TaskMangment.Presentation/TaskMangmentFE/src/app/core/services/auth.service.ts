import { Injectable } from '@angular/core';
import { Observable, throwError } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import { AuthRepository } from '../repositories/auth.repository';
import { User } from '../../models/user.model';
import { AppState } from 'app/store/app.state';
import { select, Store } from '@ngrx/store';
import { login, loginFailure, loginSuccess, logout } from 'app/store/auth/auth.actions';
import { BaseResponse } from 'app/models/base.response.model';
import { selectAuthLoading } from 'app/store/auth/auth.selectors';
import { Router } from '@angular/router';
import { AuthUser } from '../models/auth/auth-user';
import * as NavActions from '../../store/nav/nav.actions';
@Injectable({
  providedIn: 'root',
})
export class AuthService {
  public showLoader: boolean = false;
  constructor(
    private authRepository: AuthRepository,
    private store: Store<AppState>,
    private router: Router
  ) {
    this.store.pipe(select(selectAuthLoading)).subscribe(loading => {
      this.showLoader = loading;
    });
  }
  getCurrentUser() {
    const user = localStorage.getItem('userData');
    return user ? JSON.parse(user) : null;
  }

  login(userCode: string, password: string): Observable<BaseResponse<User & { token: string }>> {
    return this.authRepository.login(userCode, password).pipe(
      map(response => {
      if (response.data) {
          localStorage.setItem('authToken', response.data.token);
          localStorage.setItem('userData', JSON.stringify(response.data));
    this.store.dispatch(NavActions.initializeMenu());

          return response;
        } else {
        throw new Error(response.errorList?.join('\n') || 'Login failed');
        }
      })
    );
  }


  logout() {
    localStorage.removeItem('authToken');
    localStorage.removeItem('userData');
    
    this.store.dispatch(NavActions.clearMenu());  

    this.router.navigate(['/auth/login'], { replaceUrl: true });
  }

    forgotPassword(email: string): Observable<BaseResponse<null>> {
    return this.apiService.post<BaseResponse<null>>(
      this.service,
      'forgot-password',
      { email }
    );
  }

 verifyResetCode(request: { email: string; token: string }): Observable<BaseResponse<null>> {
    return this.apiService.post<BaseResponse<null>>(
      this.service,
      'verify-reset-code',
      request
    );
  }

  updatePassword(request: { email: string; newPassword: string }): Observable<BaseResponse<null>> {
    return this.apiService.post<BaseResponse<null>>(
      this.service,
      'reset-password',
      request
    );
  }


  private userKey = 'auth_user';

  isAuthenticated(): boolean {
    return !!localStorage.getItem('authToken');
  }
  getUser(): AuthUser | null {
    const u = localStorage.getItem('userData');
    return u ? JSON.parse(u) : null;
  }
  hasPermission(permission: string): boolean {
    return this.getUser()?.permissions.includes(permission) ?? false;
  }

  hasMinRoleLevel(level: number): boolean {
    return (this.getUser()?.roleLevel ?? 0) >= level;
  }

  getRoleLevel(): number {
    return this.getUser()?.roleLevel ?? 0;
  }
}
