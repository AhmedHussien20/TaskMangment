import { Injectable } from '@angular/core';
import { ApiService } from '../services/api.service';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model'; 
import { DepartmentAddEditDto, DepartmentGetDto, DepartmentPagedResponse } from '../models/department/department';

@Injectable({
  providedIn: 'root'
})
export class DepartmentService {

  private readonly service = 'Department';

  constructor(private api: ApiService) {}

  // GET /Department?query
  getAll(request: any): Observable<DepartmentPagedResponse> {
    const query = this.buildQuery(request);
    return this.api.get<DepartmentPagedResponse>(this.service, `?${query}`);
  }

  // GET /Department/{id}
  getById(id: number): Observable<BaseResponse<DepartmentGetDto>> {
    return this.api.get<BaseResponse<DepartmentGetDto>>(this.service, `${id}`);
  }

  // POST /Department
  create(model: DepartmentAddEditDto): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  // PUT /Department/{id}
  update(id: number, model: DepartmentAddEditDto): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

  // DELETE /Department/{id}
  delete(id: number): Observable<BaseResponse<any>> {
    return this.api.delete<BaseResponse<any>>(this.service, `${id}`);
  }

  // Build query string
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
