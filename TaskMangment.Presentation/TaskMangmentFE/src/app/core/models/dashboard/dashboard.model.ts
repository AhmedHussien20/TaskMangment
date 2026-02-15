import { TaskStatus } from "../task/task";

export interface AdminDashboardDto {
  kpis: {
    totalEmployees: number;
    activeTasks: number;
    overdueTasks: number;
    completedTasks: number;
    totalPenaltiesThisMonth: number;
    warningsThisMonth: number;
  };
  taskStatusChart: {
    items: { status: string; count: number }[];
  };
  topDelayedEmployees: {
    employeeId: number;
    employeeName: string;
    delayedTasksCount: number;
  }[];
}
export interface EmployeeDashboardDto {
  kpis: {
    myActiveTasks: number;
    dueSoonTasks: number;
    myWarnings: number;
    myPenalties: number;
  };
  myTasks: {
    taskId: number;
    title: string;
    status: string;
    dueDate: string;
    progressPercent: string;
  }[];
  performance: {
    completedTasks: number;
    totalTasks: number;
    completionRate: number;
  };
}

export interface WarningDto {
  id: number;
  taskTitle: string
  reason: string;
  createdDate: string; 
  taskStatus: string;
}

export interface PenalityDto {
  id: number;
  amount: number;
  reason: string;
  createdDate: string;
  taskTitle: string

}


export interface InProgressUpdatedTodayDto {
  taskId: number;
  title: string;
  status: string;
  dueDate: string;
  updatedBy: string;
  updatedAt: string;
}


export interface CompletedTasksTodayDto {
  employeeId: number;
  employeeName: string;
  employeeImageUrl: string;
  completedTasksCount: number;
}

export interface PendingCloseRequestDto {
  taskId: number;
  title: string;
  requestedBy: string;
  requestedAt: string;
}

export interface TaskStatusDto {
    taskId: number;
    title: string;
    status: TaskStatus;
    statusText : string;
    dueDate: Date | null;
    employees: string[];
    createdByMe: boolean

}

export interface AdminKpisExtendedDto {
  averageCompletionHours: number;
  onTimeRatePercent: number;
  highPriorityOpenTasks: number;
  penaltiesThisMonth: number;
}
export interface MyTaskDto {
  taskId: number;
  title: string;
  status: TaskStatus;
  dueDate?: string | null;    
  progressPercent: number;
  isOverdue?: boolean;    
}

export interface TodayCommentTaskDto {
  taskId: number;
  title: string;
  status: string;     
  statusText: string;
  dueDate?: Date;
  assignedBy: string;
  employees?: { name: string }[];
}
export interface  TasksPagedResponse {
  data: TodayCommentTaskDto[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}
export interface TasksRequest {
  name?: string;
  companyId?: number;
  pageIndex: number;
  pageSize: number;
  sortColumn: string;
  sortDirection: string;
}
export interface EmployeeKpisExtendedDto {
  averageCompletionHours: number;
  onTimeRatePercent: number;
}

export interface PagedResponse<T> {
  data: T[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}
export interface BranchFilterDto {
  id: number;
  name: string;
}
