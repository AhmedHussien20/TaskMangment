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

  private buildQuery(params: { fromDate?: string; toDate?: string }): string {
    const q: string[] = [];
    if (params.fromDate) q.push(`fromDate=${encodeURIComponent(params.fromDate)}`);
    if (params.toDate) q.push(`toDate=${encodeURIComponent(params.toDate)}`);
    return q.length ? `?${q.join('&')}` : '';
  }
}
