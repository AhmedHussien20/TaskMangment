import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { ApiService } from 'app/core/services/api.service';
import { SearchCriteria } from '../models/search-criteria.model';
import { Role, RoleAddEdit, RolePermissionAssign, AssignRoleToEmployee } from '../models/roles/role';

@Injectable({
  providedIn: 'root'
})
export class RoleService {

  private readonly service = 'Role';

  constructor(private api: ApiService) {}

  // POST /Role
  create(model: RoleAddEdit): Observable<BaseResponse<number>> {
    return this.api.post<BaseResponse<number>>(this.service, '', model);
  }

  // POST /Role/{roleId}/assign-permissions
  assignPermissions(roleId: number, permissionIds: number[]): Observable<BaseResponse<boolean>> {
    const body = { roleId ,permissionIds };
    return this.api.post<BaseResponse<boolean>>(this.service, 'assign-permissions', body);
  }

  // POST /Role/assign-to-employee?employeeId=X&roleId=Y
    assignToEmployee(employeeId: number, roleId: number): Observable<BaseResponse<boolean>> {
   const body = { employeeId, roleId };
     return this.api.post<BaseResponse<boolean>>(this.service, 'assign-to-employee', body);
    }

  // GET /Role/company
  getRoles(): Observable<BaseResponse<Role[]>> {
    return this.api.get<BaseResponse<Role[]>>(this.service, 'company');
  }
}