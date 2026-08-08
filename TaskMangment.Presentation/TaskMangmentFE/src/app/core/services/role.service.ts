import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

import { ApiService } from 'app/core/services/api.service';
import { BaseResponse } from 'app/models/base.response.model';

import {
  Role,
  RoleAddEdit,
  RoleNotificationUpdate,
  RolePagedResponse
} from 'app/core/models/roles/role';

import { SearchCriteria } from 'app/core/models/search-criteria.model';

@Injectable({
  providedIn: 'root'
})
export class RoleService {

  private readonly service = 'Role';

  constructor(private api: ApiService) {}

  // =============================
  // GET /Role?query
  // =============================
  getAll(request: SearchCriteria): Observable<BaseResponse<RolePagedResponse>> {
    const query = this.buildQuery(request);
    return this.api.get<BaseResponse<RolePagedResponse>>(this.service, `?${query}`);
  }

  // =============================
  // GET /Role/{id}
  // =============================
  getById(id: number): Observable<BaseResponse<Role>> {
    return this.api.get<BaseResponse<Role>>(this.service, `${id}`);
  }

  // =============================
  // POST /Role
  // =============================
  create(model: RoleAddEdit): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  // =============================
  // PUT /Role/{id}
  // =============================
  update(id: number, model: RoleAddEdit): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

  // =============================
  // PUT /Role/{id}/notifications
  // =============================
  updateNotifications(id: number, model: RoleNotificationUpdate): Observable<BaseResponse<Role>> {
    return this.api.put<BaseResponse<Role>>(this.service, `${id}/notifications`, model);
  }

  // =============================
  // DELETE /Role/{id}
  // =============================
  delete(id: number): Observable<BaseResponse<any>> {
    return this.api.delete<BaseResponse<any>>(this.service, `${id}`);
  }

  // =============================
  // QUERY BUILDER
  // =============================
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
