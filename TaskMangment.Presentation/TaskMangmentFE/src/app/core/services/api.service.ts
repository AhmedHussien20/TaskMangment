import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) {}

  get<T>(service: string, endpoint: string) {
    return this.http.get<T>(`${this.baseUrl}/${service}/${endpoint}`);
  }

  post<T>(service: string, endpoint: string, body: any) {
    return this.http.post<T>(`${this.baseUrl}/${service}/${endpoint}`, body);
  }

  put<T>(service: string, endpoint: string, body: any) {
    return this.http.put<T>(`${this.baseUrl}/${service}/${endpoint}`, body);
  }

  delete<T>(service: string, endpoint: string) {
    return this.http.delete<T>(`${this.baseUrl}/${service}/${endpoint}`);
  }

patch<T>(service: string, endpoint: string, body: any) {
  return this.http.patch<T>(`${this.baseUrl}/${service}/${endpoint}`, body);
}

getBlob(service: string, endpoint: string): Observable<Blob> {
  return this.http.get<Blob>(`${this.baseUrl}/${service}/${endpoint}`, { responseType: 'blob' as 'json' });
}


}
