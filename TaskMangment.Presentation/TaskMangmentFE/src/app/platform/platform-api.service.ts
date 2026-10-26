import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SearchCriteria } from '../core/models/search-criteria.model';
import { environment } from '../../environments/environment';
import { PlatformSession } from './platform-session.service';

@Injectable({ providedIn: 'root' })
export class PlatformApiService {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/platform`;

  login(email: string, password: string): Observable<{ data: PlatformSession }> {
    return this.http.post<{ data: PlatformSession }>(`${this.base}/login`, { email, password });
  }

  admins(criteria: SearchCriteria): Observable<any> {
    return this.http.get(`${this.base}/admins`, { params: this.params(criteria) });
  }

  createAdmin(body: { fullName: string; email: string; password: string }): Observable<any> {
    return this.http.post(`${this.base}/admins`, body);
  }

  setAdminActive(id: number, isActive: boolean): Observable<any> {
    return this.http.post(`${this.base}/admins/${id}/active`, { isActive });
  }

  organizations(criteria: SearchCriteria): Observable<any> {
    return this.http.get(`${this.base}/organizations`, { params: this.params(criteria) });
  }

  createOrganization(body: { name: string; adminFullName: string; adminEmail: string; adminPassword: string }): Observable<any> {
    return this.http.post(`${this.base}/organizations`, body);
  }

  trainees(criteria: SearchCriteria): Observable<any> {
    return this.http.get(`${this.base}/trainees`, { params: this.params(criteria) });
  }

  createTrainee(body: { companyName: string; adminFullName: string; adminEmail: string; adminPassword: string }): Observable<any> {
    return this.http.post(`${this.base}/trainees`, body);
  }

  setTraineeActive(companyId: number, isActive: boolean): Observable<any> {
    return this.http.post(`${this.base}/trainees/${companyId}/active`, { isActive });
  }

  private params(criteria: SearchCriteria): HttpParams {
    let params = new HttpParams()
      .set('searchKey', criteria.searchKey || '')
      .set('PageIndex', String(criteria.pageIndex || 1))
      .set('PageSize', String(criteria.pageSize || 10))
      .set('SortColumn', criteria.sortColumn || 'Id')
      .set('SortDirection', criteria.sortDirection || 'DESC');

    if (criteria['status'] === 'active') params = params.set('IsActive', 'true');
    if (criteria['status'] === 'inactive') params = params.set('IsActive', 'false');
    return params;
  }
}
