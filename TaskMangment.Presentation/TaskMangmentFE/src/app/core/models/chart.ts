import { TaskStatus } from "./task/task";

export interface TaskStatusCountDto {
  status: TaskStatus;
  count: number;
}

export interface EmpTaskChartItem {
  taskName: string;
  percent: string;
}

export interface EmpTasksChartResultDto {
  tasks: EmpTaskChartItem[];
  statusCounts: TaskStatusCountDto[];
}