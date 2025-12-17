export enum ExtensionRequestStatus {
  Pending = 1,
  Approved = 2,
  Rejected = 3
}

export interface TaskExtensionRequestAdd {
  reason: string;
}

export interface TaskExtensionRequestGet {
  id: number;
  taskId: number;
  taskTitle: string;
  reason: string;
  status: ExtensionRequestStatus;
  requestedAt: string;
  requestedByName?: string;
reviewedAt?: string | null;
  reviewedByName?: string;
}

export interface TaskExtensionRequestPagedResponse {
  data: TaskExtensionRequestGet[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}
