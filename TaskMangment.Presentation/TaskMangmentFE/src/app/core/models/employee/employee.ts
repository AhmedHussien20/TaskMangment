export interface Employee {
  id: number;
  fullName: string;
  branchName?: string;
  JobName?: string;
  departmentName?: string;
  email?: string;
  mobile?: string;
  roles: string[];
  lastLoginDate?: string;
  
}
export interface EnumItemDto {
  id: number;
  name: string;
}


export interface EmployeeAddEdit {
  branchId: number;
  jobId?: number;
  departmentId?: number;
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
  functionCode?: EnumItemDto
  
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
