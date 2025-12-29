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
    progressPercent: number;
  }[];
  performance: {
    completedTasks: number;
    totalTasks: number;
    completionRate: number;
  };
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
