import { BaseResponse } from "app/models/base.response.model";
import { ApiService } from "./api.service";
import { Injectable } from "@angular/core";
import { GetManagerBranchesDto } from "../models/roles/role";

@Injectable({
  providedIn: 'root'
})
export class RoleAssignmentService {

  private readonly service = 'RoleAssignment';

  constructor(private api: ApiService) {}

  // GET: /api/RoleAssignment/{roleId}/assigned-employees
  getAssignedEmployees(roleId: number, request: any) {
    const query = [
      `PageIndex=${request.pageIndex}`,
      `PageSize=${request.pageSize}`,
      `searchKey=${request.searchKey ?? ''}`,
      `IsAssigned=${request.isAssigned ?? ''}`

    ].join('&');

    return this.api.get<BaseResponse<any>>(
      this.service,
      `${roleId}/assigned-employees?${query}`
    );
  }

  // POST: /api/RoleAssignment/bulk-assign-employees?roleId=3
  bulkAssignEmployees(roleId: number, assignments: any[]) {
    return this.api.post<BaseResponse<any>>(
      this.service,
      `bulk-assign-employees?roleId=${roleId}`,
      { assignments }
    );
  }
  getManagerBranches(managerId: number) {
  return this.api.get<BaseResponse<GetManagerBranchesDto>>(
    this.service,
    `manager-branches/${managerId}`
  );
}

  setManagerBranches(managerId: number, payload: { functionCode: number; branchIds?: number[] }) {
  return this.api.post<BaseResponse<any>>(
    this.service,
    `manager-branches/${managerId}`,
    payload
  );
}

}
