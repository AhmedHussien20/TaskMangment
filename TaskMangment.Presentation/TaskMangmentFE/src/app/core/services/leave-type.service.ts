import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { BaseResponse } from 'app/models/base.response.model';
import {
  LeaveTypeGetDto,
  LeaveTypeAddEditDto
} from '../models/leave/leave-type.model';

@Injectable({ providedIn: 'root' })
export class LeaveTypeService {

  private readonly service = 'LeaveType';

  constructor(private api: ApiService) {}

  getAll(): Observable<BaseResponse<LeaveTypeGetDto[]>> {
    return this.api.get<BaseResponse<LeaveTypeGetDto[]>>(this.service, '');
  }

  getById(id: number): Observable<BaseResponse<LeaveTypeGetDto>> {
    return this.api.get<BaseResponse<LeaveTypeGetDto>>(this.service, `${id}`);
  }

  add(model: LeaveTypeAddEditDto): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  update(id: number, model: LeaveTypeAddEditDto): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

  delete(id: number): Observable<BaseResponse<boolean>> {
    return this.api.delete<BaseResponse<boolean>>(this.service, `${id}`);
  }
}
