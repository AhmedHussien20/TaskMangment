import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { Observable } from "rxjs";
import { ApiResponse } from "../models/event/calendar";
import { BranchTaskReportRowDto, EmployeeArchivedTasksReportDto, EmployeeAssignedTaskOptionDto, EmployeeAssignmentsReportDto, EmployeeCommentsReportDto, EmployeeOnTimeReportDto, EmployeeTaskCommentRowDto, EmployeeTaskTrackingReportDto, ExportType, TaskActivityReportDto, TaskDiscountReportDto, TaskMovementReportDto, TasksClosingSoonDto } from "../models/reports/reports";

@Injectable({ providedIn: 'root' })
export class ReportListService {

  private readonly service = 'ReportsList';

  constructor(private api: ApiService) {}

  getTopCommenters(fromDate?: string, toDate?: string): Observable<ApiResponse<EmployeeCommentsReportDto[]>> {
    const query = this.buildQuery({ fromDate, toDate });
    return this.api.get<ApiResponse<EmployeeCommentsReportDto[]>>(this.service, `top-commenters${query}`);
  }

  getMostAssigned(fromDate?: string, toDate?: string): Observable<ApiResponse<EmployeeAssignmentsReportDto[]>> {
    const query = this.buildQuery({ fromDate, toDate });
    return this.api.get<ApiResponse<EmployeeAssignmentsReportDto[]>>(this.service, `most-assigned${query}`);
  }

  getOnTimeCompletion(fromDate?: string, toDate?: string): Observable<ApiResponse<EmployeeOnTimeReportDto[]>> {
    const query = this.buildQuery({ fromDate, toDate });
    return this.api.get<ApiResponse<EmployeeOnTimeReportDto[]>>(this.service, `on-time-completion${query}`);
  }

  getArchivedTasks(fromDate?: string, toDate?: string): Observable<ApiResponse<EmployeeArchivedTasksReportDto[]>> {
    const query = this.buildQuery({ fromDate, toDate });
    return this.api.get<ApiResponse<EmployeeArchivedTasksReportDto[]>>(this.service, `archived-tasks${query}`);
  }
  getTaskDiscounts(employeeId: number,movementType: number, fromDate: string, toDate?: string, status?: string): Observable<ApiResponse<TaskDiscountReportDto[]>> {
  const query = this.buildQuery({ employeeId,movementType, fromDate, toDate, status });
  return this.api.get<ApiResponse<TaskDiscountReportDto[]>>(this.service, `task-discounts${query}`);
}
  getTaskActivities(fromDate: string, toDate: string): Observable<ApiResponse<TaskActivityReportDto[]>> {
    const query = this.buildQuery({ fromDate, toDate, exportType: ExportType.Pdf });
    return this.api.get<ApiResponse<TaskActivityReportDto[]>>(this.service, `task-activities${query}`);
  }
  
  getMovementReports(employeeId: number| undefined, movementType: number, reportTitle?: string): Observable<ApiResponse<TaskMovementReportDto[]>> {
  const query = this.buildQuery({ employeeId, movementType, reportTitle, exportType:ExportType.Pdf });
  return this.api.get<ApiResponse<TaskMovementReportDto[]>>(this.service, `task-movements${query}`);
}

 getClosedSoonReports(employeeId: number | undefined): Observable<ApiResponse<TasksClosingSoonDto[]>> {
  const query = this.buildQuery({ employeeId});
  return this.api.get<ApiResponse<TasksClosingSoonDto[]>>(this.service, `closing-soon-tasks${query}`);
}

getEmployeeTaskTracking(employeeId: number | undefined, fromDate: string, toDate?: string): Observable<ApiResponse<EmployeeTaskTrackingReportDto[]>> {
  const query = this.buildQuery({ employeeId, fromDate, toDate });
  return this.api.get<ApiResponse<EmployeeTaskTrackingReportDto[]>>(this.service, `employee-task-tracking${query}`);
}

getBranchTasks(branchId: number, fromDate: string, toDate?: string): Observable<ApiResponse<BranchTaskReportRowDto[]>> {
  const query = this.buildQuery({ branchId, fromDate, toDate });
  return this.api.get<ApiResponse<BranchTaskReportRowDto[]>>(this.service, `branch-tasks${query}`);
}

getEmployeeAssignedTasks(employeeId: number): Observable<ApiResponse<EmployeeAssignedTaskOptionDto[]>> {
  const query = this.buildQuery({ employeeId });
  return this.api.get<ApiResponse<EmployeeAssignedTaskOptionDto[]>>(this.service, `employee-assigned-tasks${query}`);
}

getEmployeeTaskComments(employeeId: number, taskId: number): Observable<ApiResponse<EmployeeTaskCommentRowDto[]>> {
  const query = this.buildQuery({ employeeId, exportType: ExportType.Excel }) + `&taskId=${taskId}`;
  return this.api.get<ApiResponse<EmployeeTaskCommentRowDto[]>>(this.service, `employee-task-comments${query}`);
}



 private buildQuery(params: {
  fromDate?: string;
  toDate?: string;
  status?: string;
  employeeId?: number;
  movementType?: number;
  reportTitle?: string;
  exportType?: ExportType;
  branchId?: number;           
}): string {
  const q: string[] = [];

  if (params.fromDate) q.push(`fromDate=${encodeURIComponent(params.fromDate)}`);
  if (params.toDate) q.push(`toDate=${encodeURIComponent(params.toDate)}`);
  if (params.status) q.push(`status=${encodeURIComponent(params.status)}`);
  if (params.employeeId !== undefined) q.push(`employeeId=${params.employeeId}`);
  if (params.branchId !== undefined) q.push(`branchId=${params.branchId}`);   // ✅ NEW
  if (params.movementType !== undefined) q.push(`movementType=${params.movementType}`);
  if (params.exportType !== undefined) q.push(`exportType=${params.exportType}`);
  if (params.reportTitle) q.push(`reportTitle=${encodeURIComponent(params.reportTitle)}`);

  return q.length ? `?${q.join('&')}` : '';
}


}
