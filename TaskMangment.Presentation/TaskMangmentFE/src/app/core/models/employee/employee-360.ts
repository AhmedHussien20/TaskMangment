export interface Employee360Profile {
  id: number;
  employeeCode: string;
  fullName: string;
  title?: string;
  branchName?: string;
  jobName?: string;
  departmentName?: string;
  email?: string;
  mobile?: string;
  imageUrl?: string;
  isActive: boolean;
  employeeTypeId?: number;
  employeeTypeName?: string | null;
  /** @deprecated use employeeTypeName */
  functionCode?: string | null;
  lastLoginDate?: string;
  hireDate?: string;
  qualification?: string;
  address?: string;
  nationality?: string;
  identityNumber?: string;
}

export interface Employee360Role {
  roleId: number;
  name: string;
  level: number;
}

export interface Employee360Manager {
  id: number;
  fullName: string;
  email?: string;
  jobName?: string;
}

export interface Employee360ManagerScope {
  employeeTypeIds: number[];
  /** @deprecated use employeeTypeIds */
  functionCodes?: string[];
  branches: { id: number; name: string }[];
}

export interface Employee360Kpis {
  activeTasks: number;
  completedTasks: number;
  overdueTasks: number;
  dueThisWeek: number;
  dueSoonTasks: number;
  completionRate: number;
  openWarnings: number;
  totalDiscounts: number;
  leaveBalance: number;
  currentWorkload: number;
  performanceScore: number;
  averageCompletionHours: number;
  onTimeRatePercent: number;
  totalTasks: number;
}

export interface Employee360Sidebar {
  recentComments: {
    taskId: number;
    taskTitle: string;
    commentText?: string;
    date: string;
  }[];
  upcomingDeadlines: Employee360Deadline[];
  recentNotifications: Employee360NotificationItem[];
  unreadNotifications: number;
  activeAssignments: number;
}

export interface Employee360Deadline {
  taskId: number;
  title: string;
  dueDate?: string;
  status: string;
  priority: string;
}

export interface Employee360NamedCount {
  name: string;
  count: number;
}

export interface Employee360OverviewCharts {
  taskStatusBreakdown: Employee360NamedCount[];
  monthlyProductivity: Employee360NamedCount[];
  completionTrend: Employee360NamedCount[];
}

export interface Employee360Dto {
  profile: Employee360Profile;
  roles: Employee360Role[];
  reportingManagers: Employee360Manager[];
  directManager?: Employee360Manager | null;
  managerScope?: Employee360ManagerScope | null;
  kpis: Employee360Kpis;
  sidebar: Employee360Sidebar;
  overviewCharts: Employee360OverviewCharts;
  canCreateTask: boolean;
  canEditEmployee: boolean;
}

export interface Employee360AccessDto {
  roles: Employee360Role[];
  permissions: string[];
  permissionGroups: string[];
  managerScope?: Employee360ManagerScope | null;
}

export interface Employee360PerformanceDto {
  kpis: Employee360Kpis;
  warnings: EmployeeWarningDto[];
  lateTasks: Employee360Deadline[];
  monthlyTrend: Employee360NamedCount[];
}

export interface Employee360DiscountsDto {
  totalAmount: number;
  discounts: EmployeeDeductionDto[];
}

export interface Employee360LeaveDto {
  leaveBalance: number;
  usedDays: number;
  allowedDays: number;
  pendingCount: number;
  approvedCount: number;
  rejectedCount: number;
  history: {
    id: number;
    leaveTypeName: string;
    startDate: string;
    endDate: string;
    status: string;
    notes?: string;
    createdDate: string;
  }[];
}

export interface Employee360EmailItem {
  id: number;
  date: string;
  subject: string;
  emailType: string;
  recipient: string;
  status: string;
  deliveryStatus: string;
  opened: boolean;
  clicked: boolean;
  retries: number;
  provider?: string;
  messageId?: string;
  createdBy?: string;
  deliveryTime?: string;
  openTime?: string;
  clickTime?: string;
  failureReason?: string;
  smtpResponse?: string;
  providerResponse?: string;
  taskId?: number;
  taskTitle?: string;
}

export interface Employee360NotificationItem {
  id: number;
  date: string;
  title: string;
  type: string;
  priority: string;
  channel: string;
  status: string;
  isRead: boolean;
  readTime?: string;
  delivered: boolean;
  deliveredTime?: string;
  createdBy?: string;
  taskId?: number;
  taskTitle?: string;
}

export interface Employee360CommentItem {
  id: number;
  date: string;
  commentText?: string;
  taskId: number;
  taskTitle?: string;
  attachmentCount: number;
  hasAttachments: boolean;
}

export interface EmployeeTimelineItem {
  date: string;
  type: string;
  title: string;
  description?: string;
  taskId?: number;
  taskTitle?: string;
  meta?: Record<string, string>;
}

export interface EmployeeWarningDto {
  id: number;
  reason: string;
  createdDate: string;
  taskId?: number;
  taskTitle: string;
  taskStatus: string;
}

export interface EmployeeDeductionDto {
  id: number;
  amount: number;
  reason: string;
  taskId?: number;
  taskTitle: string;
  taskStatusText?: string;
  taskAssignedByName?: string;
  taskDueDate?: string;
  createdDate: string;
  autoDiscount: boolean;
  discountType: number;
}

export interface Employee360PagedRequest {
  from?: string;
  to?: string;
  searchKey?: string;
  status?: string;
  type?: string;
  channel?: string;
  unreadOnly?: boolean;
  pageIndex?: number;
  pageSize?: number;
}
