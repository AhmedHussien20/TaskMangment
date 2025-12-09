import { Injectable } from '@angular/core';
import { Observable } from 'rxjs'; 
import { User } from '../../models/user.model';
import { ApiService } from '../services/api.service';
import { BaseResponse } from '../../models/base.response.model';

@Injectable({
  providedIn: 'root',
})
export class AuthRepository {
  constructor(private apiService: ApiService) {
    apiService.serviceName = 'Auth';
  }
  

  login(email: string, password: string): Observable<BaseResponse<{ user?: User, token: string }>> {
    return this.apiService.post<BaseResponse<{ user?: User, token: string }>>(`login`, { email, password });
  }

  logout(): void {
    localStorage.removeItem('authToken');
  }
}
