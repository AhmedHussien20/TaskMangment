import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { ApiService } from 'app/core/services/api.service';
import { SearchCriteria } from '../models/search-criteria.model';
import { Permission, PermissionAddEdit, PermissionPagedResponse } from '../models/roles/permissions';

@Injectable({
  providedIn: 'root'
})
export class PermissionService {

  private readonly service = 'Permission';

  constructor(private api: ApiService) {}

  // GET /Permission?query
  getAll(request: any): Observable<any> {
    const query = this.buildQuery(request);
    return this.api.get<any>(this.service, `?${query}`);
  }

  // POST /Permission
  create(model: PermissionAddEdit): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  // PUT /Permission/{id}
  update(id: number, model: PermissionAddEdit): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

  // DELETE /Permission/{id}
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