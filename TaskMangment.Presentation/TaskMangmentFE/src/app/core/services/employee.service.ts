import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { ApiService } from 'app/core/services/api.service';
import { SearchCriteria } from '../models/search-criteria.model';
import { Employee, EmployeeAddEdit, EmployeePagedResponse, EnumItemDto } from '../models/employee/employee';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {

  private readonly service = 'Employee';

  constructor(private api: ApiService) {}

  // GET /Employee?query
  getAll(request: any): Observable<any> {
    const query = this.buildQuery(request);
    return this.api.get<any>(this.service, `?${query}`);
  }

  // GET /Employee/{id}
  getById(id: number): Observable<BaseResponse<EmployeeAddEdit>> {
    return this.api.get<BaseResponse<EmployeeAddEdit>>(this.service, `${id}`);
  }

  // POST /Employee
  create(model: FormData): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  // PUT /Employee/{id}
  update(id: number, model: FormData): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

  // DELETE /Employee/{id}
  delete(id: number): Observable<BaseResponse<any>> {
    return this.api.delete<BaseResponse<any>>(this.service, `${id}`);
  }

  // GET /Employee/function-codes
getFunctionCodes(): Observable<BaseResponse<EnumItemDto[]>> {
  return this.api.get<BaseResponse<EnumItemDto[]>>(this.service, 'function-codes');
}



  // Build Query String
  private buildQuery(req: SearchCriteria): string {
    return [
      `searchKey=${req.searchKey ?? ''}`,
      `PageIndex=${req.pageIndex}`,
      `PageSize=${req.pageSize}`,
      `SortColumn=${req.sortColumn}`,
      `SortDirection=${req.sortDirection}`,
      `roleLevel=${(req as any).roleLevel ?? ''}`,
      `permissionCode=${(req as any).permissionCode ?? ''}`,
      `branchId=${(req as any).branchId ?? ''}`


    ].join('&');
  }
}