export interface EmployeeArchivedTasksReportDto {
  employeeName: string;
  archivedTasksCount: number;
}

export interface EmployeeOnTimeReportDto {
  employeeName: string;
  totalTasks: number;
  onTimeTasks: number;
}
export interface EmployeeAssignmentsReportDto {
  employeeName: string;
  tasksCount: number;
}
export interface EmployeeCommentsReportDto {
  employeeName: string;
  commentsCount: number;
}

export interface TaskDiscountGetReportDto {
  taskId: number;
  title: string;
  assignedBy: string;
  closedDate?: string;
  status: string;
  autoDiscount: number;
  manualDiscount: number;
  evaluation: string;
}
export interface TaskDiscountReportDto {
  taskId: number;
  title: string;
  assignedBy: string;
  closedDate?: string;
  status: string;
  autoDiscount: number;
  manualDiscount: number;
  evaluation: string;
}
export interface TaskDiscountFilterDto {
  fromDate?: string;
  toDate?: string;
  status?: string;
  employeeId?: number;
}


