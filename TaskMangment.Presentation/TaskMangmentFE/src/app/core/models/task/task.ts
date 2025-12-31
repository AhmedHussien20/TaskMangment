export enum TaskPriority {
  Low = 1,
  Medium = 2,
  High = 3
}

export enum TaskStatus {
  New = 1,
  InProgress = 2,
  Closed = 3,
  Archived = 4
}

export enum CommentAllowPeriod {
  Daily = 1,
  Weekly = 7,
  Monthly = 30
}



export interface TaskAddEdit {
  assignedEmployeeIds: number[];
  title: string;
  description: string;
  commentAllowPeriodDays?: number;
  maxWarnings: number;
  penaltyAtMaxWarnings: number;
  penaltyOnAutoClose: number;
  isShared: boolean;
  priority: TaskPriority;
  dueDate?: string;
}

export interface TaskGet {
  id: number;
  title: string;
  description: string;
  isShared: boolean;
  createdDate: string;
  assignedByName: string;
  assignEmployee
?: {
    id: number;
    name: string;
    role: string;
    isActive: boolean;
  }[];  priority: TaskPriority;
  status: TaskStatus;
  dueDate?: string;
  commentAllowPeriodDays?: number;
  maxWarnings?: number;
  penaltyAtMaxWarnings?: number;
  penaltyOnAutoClose?: number;
}

export interface TaskSummary {
  myTasks: number;
  createdByMe: number;
  inProgressTasks: number;
  newTasks: number;
}

export interface TaskPagedResponse {
  data: TaskGet[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
  summary?: TaskSummary; 
}



export interface TaskAssignedEmployee {
  employeeId: number;
  employeeName: string;
  role: string; 
  status: string;
}

export interface SimpleEmployee {
  id: number;
  fullName: string;
}