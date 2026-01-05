import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { Observable } from "rxjs";
import { ApiResponse } from "../models/event/calendar";
import { EmployeeArchivedTasksReportDto, EmployeeAssignmentsReportDto, EmployeeCommentsReportDto, EmployeeOnTimeReportDto } from "../models/reports/reports";

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

  private buildQuery(params: { fromDate?: string; toDate?: string }): string {
    const q: string[] = [];
    if (params.fromDate) q.push(`fromDate=${encodeURIComponent(params.fromDate)}`);
    if (params.toDate) q.push(`toDate=${encodeURIComponent(params.toDate)}`);
    return q.length ? `?${q.join('&')}` : '';
  }

}
