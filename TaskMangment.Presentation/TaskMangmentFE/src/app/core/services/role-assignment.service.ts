import { BaseResponse } from "app/models/base.response.model";
import { ApiService } from "./api.service";
import { Injectable } from "@angular/core";

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
      `searchKey=${request.searchKey ?? ''}`
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
}
