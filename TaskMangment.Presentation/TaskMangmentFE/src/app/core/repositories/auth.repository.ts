import { Injectable } from '@angular/core';
import { Observable } from 'rxjs'; 
import { User } from '../../models/user.model';
import { ApiService } from '../services/api.service';
import { BaseResponse } from '../../models/base.response.model';

@Injectable({
  providedIn: 'root',
})
export class AuthRepository {

  private readonly service = 'Auth';

  constructor(private apiService: ApiService) {}

  login(
    email: string,
    password: string
  ): Observable<BaseResponse<User & { token: string }>>
 {
    return this.apiService.post<
      BaseResponse<User & { token: string }>
    >(this.service, 'login', { email, password });
  }

  logout(): void {
    localStorage.removeItem('authToken');
  }
}

