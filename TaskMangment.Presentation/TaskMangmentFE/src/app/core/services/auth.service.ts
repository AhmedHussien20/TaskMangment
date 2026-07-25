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
import { ApiService } from './api.service';
import { SignalRService } from './signalr.service';
import { ToastrService } from 'ngx-toastr';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly service = 'Auth';
  public showLoader: boolean = false;
  constructor(
    private authRepository: AuthRepository,
    private store: Store<AppState>,
    private router: Router,
    private apiService: ApiService,
    private signalRService: SignalRService,
    private toastr: ToastrService
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
    this.store.dispatch(login({ userCode, password }));
    

    return this.authRepository.login(userCode, password).pipe(
      map(response => {
        if (response.data !== null) {
          localStorage.setItem('authToken', response.data.token);
          localStorage.setItem('userData', JSON.stringify(response.data));
    this.store.dispatch(NavActions.initializeMenu());

          this.store.dispatch(loginSuccess({ token: response.data.token }));
          return response;
        } else {
          this.store.dispatch(loginFailure({ error: response.errorList.join('\n') }));
          throw new Error(response.errorList.join('\n'));
        }
      }),
      catchError(error => {
        let errorMessage = error?.error?.message || error.message;
        if (error.errorList) {
          errorMessage = error.errorList.join('\n');
        }
        this.store.dispatch(loginFailure({ error: errorMessage }));
        return throwError(() => new Error(errorMessage));
      })
    );
  }

  logout() {
    this.signalRService.stop();
    this.toastr.clear();

    localStorage.removeItem('authToken');
    localStorage.removeItem('userData');
    localStorage.removeItem('userRole');
    localStorage.removeItem('currentUser');

    this.store.dispatch(logout());
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



  isAuthenticated(): boolean {
    return !!localStorage.getItem('authToken');
  }

  getUser(): AuthUser | null {
    const u = localStorage.getItem('userData');
    return u ? JSON.parse(u) : null;
  }
  hasPermission(permission: string): boolean {
    const perms = this.getUser()?.permissions;
    if (!perms?.length || !permission) return false;
    return perms.some(p => p.toUpperCase() === permission.toUpperCase());
  }

  hasAnyPermission(...permissions: string[]): boolean {
    return permissions.some(p => this.hasPermission(p));
  }

  hasAccessScope(): boolean {
    return !!this.getUser()?.hasAccessScope;
  }

  /** @deprecated Prefer hasPermission / hasAnyPermission. Kept for dual-read during migration. */
  hasMinRoleLevel(level: number): boolean {
    return (this.getUser()?.roleLevel ?? 0) >= level;
  }

  /** @deprecated Prefer hasPermission. Kept for dual-read during migration. */
  getRoleLevel(): number {
    return this.getUser()?.roleLevel ?? 0;
  }
}
