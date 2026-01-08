import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ApiService } from "./api.service";

@Injectable({ providedIn: 'root' })
export class ReportPdfService {

  private readonly service = 'Report';

  constructor(private api: ApiService) {}

  getTopCommentersPdf(fromDate?: string, toDate?: string): Observable<Blob> {
    const query = this.buildQuery({ fromDate, toDate });
    return this.api.getBlob(this.service, `top-commenters/pdf${query}`);
  }

  getMostAssignedPdf(fromDate?: string, toDate?: string): Observable<Blob> {
    const query = this.buildQuery({ fromDate, toDate });
    return this.api.getBlob(this.service, `most-assigned/pdf${query}`);
  }

  getOnTimeCompletionPdf(fromDate?: string, toDate?: string): Observable<Blob> {
    const query = this.buildQuery({ fromDate, toDate });
    return this.api.getBlob(this.service, `on-time-completion/pdf${query}`);
  }

  getArchivedTasksPdf(fromDate?: string, toDate?: string): Observable<Blob> {
    const query = this.buildQuery({ fromDate, toDate });
    return this.api.getBlob(this.service, `archived-tasks/pdf${query}`);
  }
 getTaskDiscountsPdf(employeeId: number,fromDate?: string,toDate?: string,status?: string): Observable<Blob> {
    const query = this.buildQuery({ employeeId,fromDate, toDate, status });
    return this.api.getBlob(this.service, `task-discounts/pdf${query}`);
  }

  private buildQuery(params: { fromDate?: string; toDate?: string, status?: string; employeeId?: number}): string {
    const q: string[] = [];
    if (params.fromDate) q.push(`fromDate=${encodeURIComponent(params.fromDate)}`);
    if (params.toDate) q.push(`toDate=${encodeURIComponent(params.toDate)}`);
    if (params.status) q.push(`status=${encodeURIComponent(params.status)}`);
    if (params.employeeId !== undefined) q.push(`employeeId=${params.employeeId}`);
    return q.length ? `?${q.join('&')}` : '';
  }
}
