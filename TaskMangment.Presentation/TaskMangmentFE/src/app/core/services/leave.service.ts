import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { BaseResponse } from 'app/models/base.response.model'; 
import {  LeaveAddDto,LeaveGetDto,LeavePagedResponse } from '../models/leave/leave';

@Injectable({ providedIn: 'root' })
export class LeaveService {

  private readonly service = 'Leave';

  constructor(private api: ApiService) {}

  create(model: LeaveAddDto): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  LeaveRequests(request: any): Observable<LeavePagedResponse> {
    const query = this.buildQuery(request);
    return this.api.get<LeavePagedResponse>(this.service, `?${query}`);
  }

  getById(id: number): Observable<BaseResponse<LeaveGetDto>> {
      return this.api.get<BaseResponse<LeaveGetDto>>(this.service, `${id}`);
    }

  getPending(request: any): Observable<LeavePagedResponse> {
    const query = this.buildQuery(request);
    return this.api.get<LeavePagedResponse>(this.service, `pending?${query}`);
  }

  approve(id: number): Observable<BaseResponse<boolean>> {
    return this.api.post<BaseResponse<boolean>>(this.service, `${id}/approve`, {});
  }

  reject(id: number, reason: string): Observable<BaseResponse<boolean>> {
  return this.api.post<BaseResponse<boolean>>(this.service,`${id}/reject`, {reason: reason}
);
}


 private buildQuery(req: any): string {
  const params = [];
  
  if (req.searchKey) {
    params.push(`searchKey=${encodeURIComponent(req.searchKey)}`);
  }
  
  if (req.statusId) {
    params.push(`statusId=${req.statusId}`);
  }
  
  params.push(`PageIndex=${req.pageIndex || 1}`);
  params.push(`PageSize=${req.pageSize || 10}`);
  
  if (req.sortColumn) {
    params.push(`SortColumn=${req.sortColumn}`);
  }
  if (req.sortDirection) {
    params.push(`SortDirection=${req.sortDirection}`);
  }

  if (Array.isArray(req.employeeIds) && req.employeeIds.length > 0) {
    req.employeeIds.forEach((id: number) => {
      params.push(`EmployeeIds=${id}`);
    });
  }

  return params.join('&');
}
}
