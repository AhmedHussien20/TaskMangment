export interface Role {
  id: number;
  name: string;
  description?: string;
  requiresBranchScope?: boolean;
  requiresEmployeeTypeScope?: boolean;
}

export interface RoleAddEdit {
  name: string;
  description?: string;
  requiresBranchScope?: boolean;
  requiresEmployeeTypeScope?: boolean;
}

export interface RolePermissionAssign {
  roleId: number;
  permissionIds: number[];
}

export interface AssignRoleToEmployee {
  employeeId: number;
  roleId: number;
}

export interface RoleRequest {
  pageIndex: number;
  pageSize: number;
  sortColumn: string;
  sortDirection: string;
}

export interface RolePagedResponse {
  data: Role[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;

  employeeCount: number;
  permissionCount: number;
}

export interface BranchLookupDto {
  id: number;
  name: string;
}

export interface GetManagerBranchesDto {
  managerId: number;
  branchIds: number[];
  branchLookupDtos: BranchLookupDto[];
  employeeTypeId?: number;
  /** @deprecated */
  functionCode?: number;
}
