import { TaskStatus } from "../task/task";

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
  employeeName: string,
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
export interface TaskActivityReportDto {
  taskTitleWithId: string; 
  assignedBy: string; 
  comment: string;       
  commentDate: string;    
  commentedBy: string;
}

export enum TaskMovementType {
  Outgoing = 1,
  Incoming = 2
}
export interface TaskMovementReportDto {
  reportTitle: string;
  movementType: TaskMovementType;
  taskTitleWithId: string;
  assignedBy: string;
  commentedBy: string;
  comment: string;
  commentDate: string;
}


export interface TasksClosingSoonDto {
  taskId: number;
  title: string;
  assignedBy: string;
  closedDate?: string; 
  status: TaskStatus; 
  employeeName: string;
}


