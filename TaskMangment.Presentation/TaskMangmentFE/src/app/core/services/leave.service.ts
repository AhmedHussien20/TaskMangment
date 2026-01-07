import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { BaseResponse } from 'app/models/base.response.model'; 
import {  LeaveAddDto,LeaveGetDto,LeavePagedResponse } from '../models/leave/leave';

@Injectable({ providedIn: 'root' })
export class LeaveService {

  private readonly service = 'Leave';

  constructor(private api: ApiService) {}

  /** Create leave request */
  create(model: LeaveAddDto): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  /** Logged-in employee leaves */
  getMyRequests(request: any): Observable<LeavePagedResponse> {
    const query = this.buildQuery(request);
    return this.api.get<LeavePagedResponse>(this.service, `my?${query}`);
  }

  getById(id: number): Observable<BaseResponse<LeaveGetDto>> {
      return this.api.get<BaseResponse<LeaveGetDto>>(this.service, `${id}`);
    }

  /** Manager: pending approval */
  getPending(request: any): Observable<LeavePagedResponse> {
    const query = this.buildQuery(request);
    return this.api.get<LeavePagedResponse>(this.service, `pending?${query}`);
  }

  /** Manager: approve */
  approve(id: number): Observable<BaseResponse<boolean>> {
    return this.api.post<BaseResponse<boolean>>(this.service, `${id}/approve`, {});
  }

  /** Manager: reject */
  reject(id: number, reason: string): Observable<BaseResponse<boolean>> {
    return this.api.post<BaseResponse<boolean>>(this.service, `${id}/reject`, reason);
  }

  private buildQuery(req: any): string {
    return [
      `searchKey=${req.searchKey || ''}`,
      `PageIndex=${req.pageIndex}`,
      `PageSize=${req.pageSize}`,
      `SortColumn=${req.sortColumn || ''}`,
      `SortDirection=${req.sortDirection || ''}`
    ].join('&');
  }
}
