export interface DepartmentGetDto {
  id: number;
  name: string;
  branchName: string;
  areaName: string;
  managerName: string;
  employeeCount: number;
}

export interface DepartmentAddEditDto {
  branchId: number;
  name: string;
  managerEmployeeId?: number | null;
}
export interface DepartmentPagedResponse {
  data: DepartmentGetDto[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}