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