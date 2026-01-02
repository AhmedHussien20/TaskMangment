import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';
import { 
  TaskPercentageAddEditDto, 
  TaskPercentageGetDto, 
  TaskPercentagePagedResponse 
} from '../models/task/task-percentage';
import { BaseResponse } from 'app/models/base.response.model';

@Injectable({ providedIn: 'root' })
export class TaskPercentageService {
  private readonly service = 'TaskPercentage';

  constructor(private api: ApiService) {}

  getAll(request: any): Observable<BaseResponse<TaskPercentagePagedResponse>> {
    const query = this.buildQuery(request);
    return this.api.get<BaseResponse<TaskPercentagePagedResponse>>(this.service, `?${query}`);
  }

  getById(id: number): Observable<BaseResponse<TaskPercentageGetDto>> {
    return this.api.get<BaseResponse<TaskPercentageGetDto>>(this.service, `${id}`);
  }

  create(taskId: number, model: TaskPercentageAddEditDto): Observable<BaseResponse<TaskPercentageGetDto>> {
    return this.api.post<BaseResponse<TaskPercentageGetDto>>(this.service, `${taskId}`, model);
  }

  update(id: number, model: TaskPercentageAddEditDto): Observable<BaseResponse<TaskPercentageGetDto>> {
    return this.api.put<BaseResponse<TaskPercentageGetDto>>(this.service, `${id}`, model);
  }

  delete(id: number): Observable<BaseResponse<boolean>> {
    return this.api.delete<BaseResponse<boolean>>(this.service, `${id}`);
  }

  private buildQuery(req: any): string {
    const params = new URLSearchParams();
    params.append('taskId', req.taskId?.toString() || '');
    params.append('searchKey', req.searchKey || '');
    params.append('pageIndex', req.pageIndex?.toString() || '1');
    params.append('pageSize', req.pageSize?.toString() || '10');
    params.append('sortColumn', req.sortColumn || 'createdDate');
    params.append('sortDirection', req.sortDirection || 'desc');
    return params.toString();
  }
}
