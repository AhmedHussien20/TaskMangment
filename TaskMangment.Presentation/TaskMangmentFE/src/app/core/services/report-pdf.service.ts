import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ApiService } from "./api.service";
import { ExportType } from "../models/reports/reports";

@Injectable({ providedIn: 'root' })
export class ReportPdfService {

  private readonly service = 'Report';

  constructor(private api: ApiService) {}

  getTopCommentersPdf(exportType: ExportType, fromDate?: string, toDate?: string, roleId?: number, roleTitle?: string, status?: string): Observable<Blob> {
    const query = this.buildQuery({ exportType, fromDate, toDate, roleId, roleTitle, status });
    return this.api.getBlob(this.service, `top-commenters/pdf${query}`);
  }

  getMostAssignedPdf(exportType: ExportType, fromDate?: string, toDate?: string, roleId?: number, roleTitle?: string, status?: string): Observable<Blob> {
    const query = this.buildQuery({ exportType, fromDate, toDate, roleId, roleTitle, status });
    return this.api.getBlob(this.service, `most-assigned/pdf${query}`);
  }

  getOnTimeCompletionPdf(exportType: ExportType, fromDate?: string, toDate?: string, roleId?: number, roleTitle?: string, status?: string): Observable<Blob> {
    const query = this.buildQuery({ exportType, fromDate, toDate, roleId, roleTitle, status });
    return this.api.getBlob(this.service, `on-time-completion/pdf${query}`);
  }

  getArchivedTasksPdf(exportType: ExportType, fromDate?: string, toDate?: string, roleId?: number, roleTitle?: string, status?: string): Observable<Blob> {
    const query = this.buildQuery({ exportType, fromDate, toDate, roleId, roleTitle, status });
    return this.api.getBlob(this.service, `archived-tasks/pdf${query}`);
  }

  getTaskDiscountsPdf(exportType: ExportType, employeeId: number, movementType: number, fromDate?: string, toDate?: string, status?: string): Observable<Blob> {
    const query = this.buildQuery({ exportType, employeeId, movementType, fromDate, toDate, status });
    return this.api.getBlob(this.service, `task-discounts/pdf${query}`);
  }

  getTaskActivitiesPdf(fromDate: string, toDate: string, exportType: ExportType, roleId?: number, roleTitle?: string, status?: string): Observable<Blob> {
    const query = this.buildQuery({ fromDate, toDate, exportType, roleId, roleTitle, status });
    return this.api.getBlob(this.service, `task-activities/pdf${query}`);
  }

  getTaskMovementReportsPdf(exportType: ExportType, employeeId: number | undefined, movementType: number, reportTitle?: string, status?: string): Observable<Blob> {
    const query = this.buildQuery({ employeeId, movementType, reportTitle, exportType, status });
    return this.api.getBlob(this.service, `task-movements/pdf${query}`);
  }

  getTaskClosedSoonReportsPdf(exportType: ExportType, employeeId: number | undefined, status?: string): Observable<Blob> {
    const query = this.buildQuery({ exportType, employeeId, status });
    return this.api.getBlob(this.service, `closing-soon-tasks/pdf${query}`);
  }

  getEmployeeTaskTrackingPdf(
    exportType: ExportType, employeeId: number, fromDate: string, toDate?: string, status?: string
  ): Observable<Blob> {
    const query = this.buildQuery({ exportType, employeeId, fromDate, toDate, status });
    return this.api.getBlob(this.service, `employee-task-tracking/pdf${query}`);
  }

  getBranchTasksPdf(
    exportType: ExportType,
    branchId: number,
    fromDate: string,
    toDate?: string,
    status?: string
  ): Observable<Blob> {
    const query = this.buildQuery({ exportType, branchId, fromDate, toDate, status });
    return this.api.getBlob(this.service, `branch-tasks/pdf${query}`);
  }

  getEmployeeTaskCommentsPdf(
    exportType: ExportType,
    employeeId: number,
    taskId: number
  ): Observable<Blob> {
    const query = this.buildQuery({ exportType, employeeId }) + `&taskId=${taskId}`;
    return this.api.getBlob(this.service, `employee-task-comments/pdf${query}`);
  }

  getEmployeeTotalDiscountsPdf(
    exportType: ExportType,
    branchId?: number,
    roleId?: number,
    roleTitle?: string,
    fromDate?: string,
    toDate?: string,
    status?: string
  ): Observable<Blob> {
    const query = this.buildQuery({ exportType, branchId, roleId, roleTitle, fromDate, toDate, status });
    return this.api.getBlob(this.service, `employee-total-discounts/pdf${query}`);
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
