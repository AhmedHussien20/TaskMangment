import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Employee, EmployeePagedResponse, EmployeeRequest } from '../models/employee/employee';
import { SearchCriteria } from '../models/search-criteria.model';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
@Injectable({
  providedIn: 'root'
})
export class EmployeeService {

  private readonly service = 'Employee';

  constructor(private api: ApiService) {}

  getAll(request: any): Observable<any> {
    const query = this.buildQuery(request);
    return this.api.get<EmployeePagedResponse>(this.service, `?${query}`);
  }

  getById(id: number): Observable<Employee>  {
    return this.api.get<Employee>(this.service, `${id}`);
  }

  create(model: any) {
    return this.api.post<any>(this.service, '', model);
  }

  update(id: number, model: any) {
    return this.api.put<any>(this.service, `${id}`, model);
  }

  delete(id: number) {
    return this.api.delete<any>(this.service, `${id}`);
  }

  private buildQuery(req: any): string {
    return [
      `searchKey=${req.searchKey ?? ''}`,
      `PageIndex=${req.pageIndex}`,
      `PageSize=${req.pageSize}`,
      `SortColumn=${req.sortColumn}`,
      `SortDirection=${req.sortDirection}`
    ].join('&');
  }
}

