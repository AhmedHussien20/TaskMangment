export interface Role {
  id: number;
  name: string;
  description?: string;
}

export interface RoleAddEdit {
  companyId?: number;
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