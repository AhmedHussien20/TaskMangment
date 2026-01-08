import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { Observable } from "rxjs";
import { ApiResponse } from "../models/event/calendar";
import { EmployeeArchivedTasksReportDto, EmployeeAssignmentsReportDto, EmployeeCommentsReportDto, EmployeeOnTimeReportDto, TaskDiscountReportDto } from "../models/reports/reports";

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
  getTaskDiscounts(employeeId: number, fromDate: string, toDate?: string, status?: string): Observable<ApiResponse<TaskDiscountReportDto[]>> {
  const query = this.buildQuery({ employeeId, fromDate, toDate, status });
  return this.api.get<ApiResponse<TaskDiscountReportDto[]>>(this.service, `task-discounts${query}`);
}


 private buildQuery(params: { fromDate?: string; toDate?: string; status?: string; employeeId?: number }): string {
  const q: string[] = [];
  if (params.fromDate) q.push(`fromDate=${encodeURIComponent(params.fromDate)}`);
  if (params.toDate) q.push(`toDate=${encodeURIComponent(params.toDate)}`);
  if (params.status) q.push(`status=${encodeURIComponent(params.status)}`);
  if (params.employeeId !== undefined) q.push(`employeeId=${params.employeeId}`);
  return q.length ? `?${q.join('&')}` : '';
}


}
