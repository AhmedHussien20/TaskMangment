export interface WarningAddEditDto {
  issuedEmployeeId: number;
  reason: string;
}
export interface WarningGetDto {
  id: number;
  taskId: number;
  taskTitle: string;
  taskAssignmentId: number;
  reason: string;
  issuedAt: string;
  issuedEmployeeName: string;
  issuedByName: string;
}

export interface WarningAddEditDto {
  issuedEmployeeId: number;
  reason: string;
}

export interface WarningPagedResponse {
  data: WarningGetDto[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}
