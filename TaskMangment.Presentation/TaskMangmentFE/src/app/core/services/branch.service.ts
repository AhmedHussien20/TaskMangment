import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiService } from 'app/core/services/api.service';

import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { BranchAddEditDto, BranchGetDto, BranchPagedResponse } from '../models/branch/branch';
import { SearchCriteria } from '../models/search-criteria.model';
@Injectable({
  providedIn: 'root'
})
export class BranchService {

  private readonly service = 'Branch';

  constructor(private api: ApiService) {}

  // GET /Branch?query
  getAll(request: any): Observable<any> {
    const query = this.buildQuery(request);
    return this.api.get<any>(this.service, `?${query}`);
  }

  // GET /Branch/{id}
  getById(id: number): Observable<BaseResponse<BranchGetDto>> {
    return this.api.get<BaseResponse<BranchGetDto>>(this.service, `${id}`);
  }

  // POST /Branch
  create(model: BranchAddEditDto): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  // PUT /Branch/{id}
  update(id: number, model: BranchAddEditDto): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

  // DELETE /Branch/{id}
  delete(id: number): Observable<BaseResponse<any>> {
    return this.api.delete<BaseResponse<any>>(this.service, `${id}`);
  }

  // Build Query String
  private buildQuery(req: SearchCriteria): string {
    return [
      `searchKey=${req.searchKey ?? ''}`,
      `PageIndex=${req.pageIndex}`,
      `PageSize=${req.pageSize}`,
      `SortColumn=${req.sortColumn}`,
      `SortDirection=${req.sortDirection}`
    ].join('&');
  }
}
