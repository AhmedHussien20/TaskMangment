export enum CloseRequestStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3
}

export interface TaskCloseRequestAdd {
  message: string;
}

export interface TaskCloseRequestGet {
  id: number;
  taskId: number;
  taskTitle: string;
  message: string;
  status: CloseRequestStatus;
  requestedAt: string;
  reviewedAt?: string;
  reviewedByName?: string;
  requestedByName?: string;
  closeRequestText : string
}

export interface TaskCloseRequestPagedResponse {
  data: TaskCloseRequestGet[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}
