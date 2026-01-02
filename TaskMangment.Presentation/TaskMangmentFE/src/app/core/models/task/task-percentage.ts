export interface TaskPercentageAddEditDto {
  achievementPercent: string;
}

export interface TaskPercentageGetDto {
  id: number;
  taskId: number;
  taskTitle: string;
  employeeId?: number;
  employeeName?: string;
  achievementPercent: string;
  createdDate: string;
}

export interface TaskPercentagePagedResponse {
  data: TaskPercentageGetDto[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}
