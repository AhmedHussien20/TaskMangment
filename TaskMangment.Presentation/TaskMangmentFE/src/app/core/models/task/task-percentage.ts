export interface TaskPercentageAddEditDto {
  achievementPercent: string;
  achievementReason: string;
}

export interface TaskPercentageGetDto {
  id: number;
  taskId: number;
  taskTitle: string;
  employeeId?: number;
  employeeName?: string;
  achievementPercent: string;
  achievementReason: string;
  createdDate: string;
}

export interface TaskPercentagePagedResponse {
  data: TaskPercentageGetDto[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}
