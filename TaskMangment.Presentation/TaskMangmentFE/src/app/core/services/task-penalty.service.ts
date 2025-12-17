import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';
import { DiscountListDto, DiscountPagedResponse, DiscountAddEditDto } from '../models/task/task-penalty';
import { BaseResponse } from '../models/base.response';

@Injectable({ providedIn: 'root' })
export class TaskPenaltyService {

  private readonly service = 'TaskPenalty';

  constructor(private api: ApiService) {}

  getAll(request: any): Observable<BaseResponse<DiscountPagedResponse>> {
    const query = this.buildQuery(request);
    return this.api.get<BaseResponse<DiscountPagedResponse>>(this.service, `?${query}`);
  }

  
  getById(id: number): Observable<BaseResponse<DiscountListDto>> {
      return this.api.get<BaseResponse<DiscountListDto>>(this.service, `${id}`);
    }
  
    create(taskId: number, model: DiscountAddEditDto): Observable<BaseResponse<DiscountListDto>> {
      return this.api.post<BaseResponse<DiscountListDto>>(this.service, `${taskId}`, model);
    }
  
    update(id: number, model: DiscountAddEditDto): Observable<BaseResponse<DiscountListDto>> {
      return this.api.put<BaseResponse<DiscountListDto>>(this.service, `${id}`, model);
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
