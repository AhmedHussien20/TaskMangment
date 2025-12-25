export interface Employee {
  id: number;
  fullName: string;
  branchName?: string;
  email?: string;
  mobile?: string;
  roles: string[];
}

export interface EmployeeAddEdit {
  branchId?: number;
  title?: string;
  fullName: string;
  nationality?: string;
  identityNumber?: string;
  mobile?: string;
  address?: string;
  qualification?: string;
  roleIds: number[];
  email?: string;
  password?: string;
  attachments?: File; 
}

export interface EmployeeRequest {
  name?: string;
  branchId?: number;
  pageIndex: number;
  pageSize: number;
  sortColumn: string;
  sortDirection: string;
}

export interface EmployeePagedResponse {
  data: Employee[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}
