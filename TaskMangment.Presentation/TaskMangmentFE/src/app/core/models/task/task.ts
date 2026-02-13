import { TaskCloseRequestGet } from "./task-close-request";
import { TaskExtensionRequestGet } from "./task-extension-request";

export enum TaskPriority {
  Low = 1,
  Medium = 2,
  High = 3
}

export enum TaskStatus {
  New = 1,
  InProgress = 2,
  Closed = 3,
  Archived = 4,
  AutoClose = 5
}

export enum CommentAllowPeriod {
  Daily = 1,
  Weekly = 7,
  Monthly = 30
}

 export enum CloseReason
 {
     Auto = 1,
Manual = 2,
Rejected = 3,
Admin = 4,
 }



export interface TaskAddEdit {
  assignedEmployeeIds: number[];
  title: string;
  description: string;
  commentAllowPeriodDays?: number;
  maxWarnings: number;
  penaltyAtMaxWarnings: number;
  penaltyOnAutoClose: number;
  penaltyOnStopComment? : number;
  isShared: boolean;
  requireUploadFile: boolean;
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
  penaltyOnStopComment? : number;
  ClosedAt? : string
  ClosedByUserId? :number
  CloseReason? : CloseReason 
  requireUploadFile: boolean;
  newDate?: string;
  numberOfExtensions?: number;
  createdByMe: boolean
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
  isRead: boolean | false
}

export interface SimpleEmployee {
  id: number;
  fullName: string;
}

export interface TaskRequests {
  taskId: number;
  extensionRequests: TaskExtensionRequestGet[];
  closeRequests: TaskCloseRequestGet[];
}