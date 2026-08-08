import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { BaseResponse } from 'app/models/base.response.model';
import {
  EmployeeTypeGetDto,
  EmployeeTypeAddEditDto
} from '../models/employee/employee-type.model';
import { EnumItemDto } from '../models/employee/employee';

@Injectable({ providedIn: 'root' })
export class EmployeeTypeService {

  private readonly service = 'EmployeeType';

  constructor(private api: ApiService) {}

  getAll(request: any): Observable<any> {
    const query = this.buildQuery(request);
    return this.api.get<any>(this.service, `?${query}`);
  }

  getById(id: number): Observable<BaseResponse<EmployeeTypeGetDto>> {
    return this.api.get<BaseResponse<EmployeeTypeGetDto>>(this.service, `${id}`);
  }

  add(model: EmployeeTypeAddEditDto): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  update(id: number, model: EmployeeTypeAddEditDto): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

  delete(id: number): Observable<BaseResponse<boolean>> {
    return this.api.delete<BaseResponse<boolean>>(this.service, `${id}`);
  }

  lookup(): Observable<BaseResponse<EnumItemDto[]>> {
    return this.api.get<BaseResponse<EnumItemDto[]>>(this.service, 'lookup');
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
