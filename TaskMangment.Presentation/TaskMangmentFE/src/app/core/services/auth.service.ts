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

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  public showLoader:boolean=false;
  constructor(
    private authRepository: AuthRepository,
    private store: Store<AppState>
  ) {
    this.store.pipe(select(selectAuthLoading)).subscribe(loading => {
      this.showLoader = loading;
    });
  }
  login(userCode: string, password: string): Observable<BaseResponse<{ user?: User, token: string }>> {
    this.store.dispatch(login({ userCode, password }));
    return this.authRepository.login(userCode, password).pipe(
      map(response => {
        if (response.data !== null) {
          localStorage.setItem('authToken', response.data.token);
          localStorage.setItem('userData', JSON.stringify(response.data));
          this.store.dispatch(loginSuccess({ token: response.data.token }));
          return response;
        } else {
          this.store.dispatch(loginFailure({ error: response.errorList.join('\n') }));
          throw new Error(response.errorList.join('\n'));
        }
      }),
      catchError(error => {
        let errorMessage = error.message;
        if (error.errorList) {
          errorMessage = error.errorList.join('\n');
        }
        this.store.dispatch(loginFailure({ error: errorMessage }));
        return throwError(() => new Error(errorMessage));
      })
    );
  }

  logout(): void {
    this.authRepository.logout();
    console.log(localStorage.getItem('authToken'));
    this.store.dispatch(logout());
  }

  isAuthenticated(): boolean {
    return !!localStorage.getItem('authToken');
  }
}
