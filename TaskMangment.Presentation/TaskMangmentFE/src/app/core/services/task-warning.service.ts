import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';
import { WarningGetDto, WarningAddEditDto, WarningPagedResponse } from '../models/task/task-warning';
import { BaseResponse } from 'app/models/base.response.model';

@Injectable({ providedIn: 'root' })
export class TaskWarningService {
  private readonly service = 'TaskWarning';

  constructor(private api: ApiService) {}

  getAll(request: any): Observable<BaseResponse<WarningPagedResponse>> {
    const query = this.buildQuery(request);
    return this.api.get<BaseResponse<WarningPagedResponse>>(this.service, `?${query}`);
  }

  getById(id: number): Observable<BaseResponse<WarningGetDto>> {
    return this.api.get<BaseResponse<WarningGetDto>>(this.service, `${id}`);
  }

  create(taskId: number, model: WarningAddEditDto): Observable<BaseResponse<WarningGetDto>> {
    return this.api.post<BaseResponse<WarningGetDto>>(this.service, `${taskId}`, model);
  }

  update(id: number, model: WarningAddEditDto): Observable<BaseResponse<WarningGetDto>> {
    return this.api.put<BaseResponse<WarningGetDto>>(this.service, `${id}`, model);
  }

  delete(id: number): Observable<BaseResponse<boolean>> {
    return this.api.delete<BaseResponse<boolean>>(this.service, `${id}`);
  }

  private buildQuery(req: any): string {
    return [
      `taskId=${req.taskId}`,
      `searchKey=${req.searchKey || ''}`,
      `pageIndex=${req.pageIndex}`,
      `pageSize=${req.pageSize}`,
      `sortColumn=${req.sortColumn}`,
      `sortDirection=${req.sortDirection}`
    ].join('&');
  }
}
