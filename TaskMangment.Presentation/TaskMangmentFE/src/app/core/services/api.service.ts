import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private baseUrl = environment.apiUrl;
  public serviceName = '';

  constructor(private http: HttpClient) {}

  get<T>(endpoint: string, options?: { params?: HttpParams }): Observable<T> {
      return this.http.get<T>(`${this.baseUrl}/${this.serviceName}/${endpoint}`, { params: options?.params });
  }

  post<T>(endpoint: string, body: any): Observable<T> {
    return this.http.post<T>(
      `${this.baseUrl}/${this.serviceName}/${endpoint}`,
      body
    );
  }

  put<T>(endpoint: string, body: any): Observable<T> {
    return this.http.put<T>(`${this.baseUrl}/${this.serviceName}/${endpoint}`, body);
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.http.delete<T>(
      `${this.baseUrl}/${this.serviceName}/${endpoint}`
    );
  }

  patch<T>(endpoint: string, body: any): Observable<T> {
    return this.http.patch<T>(
      `${this.baseUrl}/${this.serviceName}/${endpoint}`,
      body
    );
  }
}
