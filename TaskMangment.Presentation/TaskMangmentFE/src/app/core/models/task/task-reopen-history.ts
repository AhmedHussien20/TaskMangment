export interface TaskReopenHistory {
  id: number;
  taskId: number;
  reopenedByEmployeeId: number;
  reopenedByName: string;
  reopenedAt: string;
  previousStatus: number;
  previousClosedAt?: string;
  previousCloseReason?: number;
  previousDueDate?: string;
  newDueDate?: string;
}
