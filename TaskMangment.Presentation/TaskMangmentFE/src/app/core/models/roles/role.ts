export interface Role {
  id: number;
  name: string;
  description?: string;
}

export interface RoleAddEdit {
  name: string;
  description?: string;
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
}