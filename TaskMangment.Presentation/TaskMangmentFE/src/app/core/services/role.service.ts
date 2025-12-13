import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { ApiService } from 'app/core/services/api.service';
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

  // POST /Role/AssignPermissions
  assignPermissions(model: RolePermissionAssign): Observable<BaseResponse<boolean>> {
    return this.api.post<BaseResponse<boolean>>(this.service, 'AssignPermissions', model);
  }

  // POST /Role/AssignToEmployee
  assignToEmployee(model: AssignRoleToEmployee): Observable<BaseResponse<boolean>> {
    return this.api.post<BaseResponse<boolean>>(this.service, 'AssignToEmployee', model);
  }

  // GET /Role/company/{companyId}
  getRoles(companyId: number): Observable<BaseResponse<Role[]>> {
    return this.api.get<BaseResponse<Role[]>>(this.service, `company/${companyId}`);
  }
}