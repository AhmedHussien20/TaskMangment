export interface AuthUser {
  userId: number;
  fullName: string;
  email: string;
  roleLevel: number;
  roleLevelName: string;
  roles: string[];
  permissions: string[];
  token: string;
  employeeTypeId?: number;
  /** @deprecated use employeeTypeId; kept for menu/nav compatibility */
  functionCode?: number;
  hasAccessScope?: boolean;
}
