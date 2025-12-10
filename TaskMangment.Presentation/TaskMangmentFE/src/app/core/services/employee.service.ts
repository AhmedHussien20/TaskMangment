import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Employee, EmployeePagedResponse } from '../models/employee/employee';
import { SearchCriteria } from '../models/search-criteria.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
} )
export class EmployeeService {

  constructor(private http: HttpClient, private api: ApiService ) {
    this.api.serviceName = 'Employee';
  }

  // GET /Employee?...
  getAll(request: any): Observable<EmployeePagedResponse> {
    this.api.serviceName = 'Employee'; 
    const query = this.buildQuery(request);
    return this.api.get<EmployeePagedResponse>(`?${query}`);
  }
  
  // GET /Employee/{id}
  getById(id: number): Observable<Employee> {
    this.api.serviceName = 'Employee'; 
    return this.api.get<Employee>(`/${id}`);
  }

  // POST /Employee
  create(model: any) {
    this.api.serviceName = 'Employee'; 
    return this.api.post('', model);
  }

  // PUT /Employee/{id}
  update(id: number, model: any) {
    this.api.serviceName = 'Employee';
    return this.api.put(`/${id}`, model);
  }

  // DELETE /Employee/{id}
  delete(id: number) {
    this.api.serviceName = 'Employee';
    return this.api.delete(`/${id}`);
  }

  // Build query string
  private buildQuery(req: SearchCriteria): string {
     const params: string[] = []; 
     params.push(`searchKey=${req.searchKey}`);
     params.push(`PageIndex=${req.pageIndex}`);
     params.push(`PageSize=${req.pageSize}`);
     params.push(`SortColumn=${req.sortColumn}`);
     params.push(`SortDirection=${req.sortDirection}`);
 
     return params.join('&');
   }
}
