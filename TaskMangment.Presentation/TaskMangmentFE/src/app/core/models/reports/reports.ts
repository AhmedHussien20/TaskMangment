import { TaskStatus } from "../task/task";

export interface EmployeeArchivedTasksReportDto {
  employeeName: string;
  archivedTasksCount: number;
  totalTasks: number;
  archiveRate: number;
}

export interface EmployeeOnTimeReportDto {
  employeeName: string;
  totalClosedTasks: number;
  onTimeTasks: number;
  lateTasks: number;
  commitmentPercentage: number;
}
export interface EmployeeAssignmentsReportDto {
  employeeName: string;
  totalTasks: number;
  newTasks: number;
  inProgressTasks: number;
  closedTasks: number;
  overdueTasks: number;
  closingSoonTasks: number;
  completionRate: number;
}
           
export interface EmployeeCommentsReportDto {
  employeeName: string;
  commentsCount: number;
  distinctTasksCount: number;
  avgCommentsPerTask: number;
  lastCommentDate: string;
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
  commentDate: string;
}


export interface TasksClosingSoonDto {
  taskId: number;
  title: string;
  assignedBy: string;
  closedDate?: string; 
  status: TaskStatus; 
  employeeName: string;
  branchName: string;
  areaName: string;
  companyName: string;
  dueDate: string;
}


