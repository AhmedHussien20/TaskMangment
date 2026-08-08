import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { Observable } from "rxjs";
import { ApiResponse } from "../models/event/calendar";
import { BranchTaskReportRowDto, EmployeeArchivedTasksReportDto, EmployeeAssignedTaskOptionDto, EmployeeAssignmentsReportDto, EmployeeCommentsReportDto, EmployeeOnTimeReportDto, EmployeeTaskCommentRowDto, EmployeeTaskTrackingReportDto, EmployeeTotalDiscountReportRowDto, ExportType, TaskActivityReportDto, TaskDiscountReportDto, TaskMovementReportDto, TasksClosingSoonDto } from "../models/reports/reports";

@Injectable({ providedIn: 'root' })
export class ReportListService {

  private readonly service = 'ReportsList';

  constructor(private api: ApiService) {}

  getTopCommenters(fromDate?: string, toDate?: string, roleId?: number, roleTitle?: string, status?: string): Observable<ApiResponse<EmployeeCommentsReportDto[]>> {
    const query = this.buildQuery({ fromDate, toDate, roleId, roleTitle, status });
    return this.api.get<ApiResponse<EmployeeCommentsReportDto[]>>(this.service, `top-commenters${query}`);
  }

  getMostAssigned(fromDate?: string, toDate?: string, roleId?: number, roleTitle?: string, status?: string): Observable<ApiResponse<EmployeeAssignmentsReportDto[]>> {
    const query = this.buildQuery({ fromDate, toDate, roleId, roleTitle, status });
    return this.api.get<ApiResponse<EmployeeAssignmentsReportDto[]>>(this.service, `most-assigned${query}`);
  }

  getOnTimeCompletion(fromDate?: string, toDate?: string, roleId?: number, roleTitle?: string, status?: string): Observable<ApiResponse<EmployeeOnTimeReportDto[]>> {
    const query = this.buildQuery({ fromDate, toDate, roleId, roleTitle, status });
    return this.api.get<ApiResponse<EmployeeOnTimeReportDto[]>>(this.service, `on-time-completion${query}`);
  }

  getArchivedTasks(fromDate?: string, toDate?: string, roleId?: number, roleTitle?: string, status?: string): Observable<ApiResponse<EmployeeArchivedTasksReportDto[]>> {
    const query = this.buildQuery({ fromDate, toDate, roleId, roleTitle, status });
    return this.api.get<ApiResponse<EmployeeArchivedTasksReportDto[]>>(this.service, `archived-tasks${query}`);
  }

  getTaskDiscounts(employeeId: number, movementType: number, fromDate: string, toDate?: string, status?: string): Observable<ApiResponse<TaskDiscountReportDto[]>> {
    const query = this.buildQuery({ employeeId, movementType, fromDate, toDate, status });
    return this.api.get<ApiResponse<TaskDiscountReportDto[]>>(this.service, `task-discounts${query}`);
  }

  getTaskActivities(fromDate: string, toDate: string, roleId?: number, roleTitle?: string, status?: string): Observable<ApiResponse<TaskActivityReportDto[]>> {
    const query = this.buildQuery({ fromDate, toDate, roleId, roleTitle, status, exportType: ExportType.Pdf });
    return this.api.get<ApiResponse<TaskActivityReportDto[]>>(this.service, `task-activities${query}`);
  }

  getMovementReports(employeeId: number | undefined, movementType: number, reportTitle?: string, status?: string): Observable<ApiResponse<TaskMovementReportDto[]>> {
    const query = this.buildQuery({ employeeId, movementType, reportTitle, status, exportType: ExportType.Pdf });
    return this.api.get<ApiResponse<TaskMovementReportDto[]>>(this.service, `task-movements${query}`);
  }

  getClosedSoonReports(employeeId: number | undefined, status?: string): Observable<ApiResponse<TasksClosingSoonDto[]>> {
    const query = this.buildQuery({ employeeId, status });
    return this.api.get<ApiResponse<TasksClosingSoonDto[]>>(this.service, `closing-soon-tasks${query}`);
  }

  getEmployeeTaskTracking(employeeId: number | undefined, fromDate: string, toDate?: string, status?: string): Observable<ApiResponse<EmployeeTaskTrackingReportDto[]>> {
    const query = this.buildQuery({ employeeId, fromDate, toDate, status });
    return this.api.get<ApiResponse<EmployeeTaskTrackingReportDto[]>>(this.service, `employee-task-tracking${query}`);
  }

  getBranchTasks(branchId: number, fromDate: string, toDate?: string, status?: string): Observable<ApiResponse<BranchTaskReportRowDto[]>> {
    const query = this.buildQuery({ branchId, fromDate, toDate, status });
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

  getEmployeeTotalDiscounts(branchId?: number, roleId?: number, roleTitle?: string, fromDate?: string, toDate?: string, status?: string): Observable<ApiResponse<EmployeeTotalDiscountReportRowDto[]>> {
    const query = this.buildQuery({ branchId, roleId, roleTitle, fromDate, toDate, status });
    return this.api.get<ApiResponse<EmployeeTotalDiscountReportRowDto[]>>(this.service, `employee-total-discounts${query}`);
  }

  getFilterRoles(): Observable<ApiResponse<{ id: number; name: string; level: number }[]>> {
    return this.api.get<ApiResponse<{ id: number; name: string; level: number }[]>>(this.service, 'filter-roles');
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
    roleId?: number;
    roleTitle?: string;
  }): string {
    const q: string[] = [];

    if (params.fromDate) q.push(`fromDate=${encodeURIComponent(params.fromDate)}`);
    if (params.toDate) q.push(`toDate=${encodeURIComponent(params.toDate)}`);
    if (params.status) q.push(`status=${encodeURIComponent(params.status)}`);
    if (params.employeeId !== undefined) q.push(`employeeId=${params.employeeId}`);
    if (params.branchId !== undefined) q.push(`branchId=${params.branchId}`);
    if (params.roleId !== undefined) q.push(`roleId=${params.roleId}`);
    if (params.movementType !== undefined) q.push(`movementType=${params.movementType}`);
    if (params.exportType !== undefined) q.push(`exportType=${params.exportType}`);
    if (params.reportTitle) q.push(`reportTitle=${encodeURIComponent(params.reportTitle)}`);
    if (params.roleTitle) q.push(`roleTitle=${encodeURIComponent(params.roleTitle)}`);

    return q.length ? `?${q.join('&')}` : '';
  }
}
