import { Injectable } from '@angular/core';
import { BaseResponse } from 'app/models/base.response.model';
import { ApiService } from './api.service';

@Injectable({
  providedIn: 'root'
})
export class RolePermissionService {
  private readonly service = 'RolePermission';

  constructor(private api: ApiService) {}

  // GET: /api/RolePermission/{roleId}/assigned-permissions
  getAssignedPermissions(roleId: number, request: any) {
    const query = [
      `PageIndex=${request.pageIndex}`,
      `PageSize=${request.pageSize}`,
      `searchKey=${request.searchKey || ''}`,
    ].join('&');

    return this.api.get<BaseResponse<any>>(
      this.service,
      `${roleId}/assigned-permissions?${query}`
    );
  }

  // POST: /api/RolePermission/bulk-assign-permissions?roleId={roleId}
  bulkAssignPermissions(roleId: number, assignments: any[]) {
    return this.api.post<BaseResponse<any>>(
      this.service,
      `bulk-assign-permissions?roleId=${roleId}`,
      { assignments }
    );
  }
}