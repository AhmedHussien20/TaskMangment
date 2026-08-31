export enum LeaveStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3
}

export interface LeaveAddDto {
  leaveTypeId: number;
  startDate: string;
  endDate: string;
  notes?: string | null;
}

export interface LeaveGetDto {
  id: number;
  employeeId: number;
  employeeName: string;
  leaveTypeName: string;
  leaveTypeId : number;
  startDate: string;
  endDate: string;
  status: LeaveStatus;
  statusName: string;
  rejectionReason?: string | null;
  notes?: string | null;
  canDelete?: boolean;
}

export interface LeavePagedResponse {
  data: LeaveGetDto[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}
