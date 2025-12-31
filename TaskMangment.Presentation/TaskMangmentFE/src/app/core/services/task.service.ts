import { Injectable } from "@angular/core";
import { TaskAddEdit, TaskAssignedEmployee, TaskGet, TaskPagedResponse, TaskRequests } from "../models/task/task";
import { ApiService } from "./api.service";
import { BaseResponse } from 'app/models/base.response.model'; 
import { Observable } from "rxjs";

@Injectable({ providedIn: 'root' })
export class TaskService {

  private readonly service = 'Task';

  constructor(private api: ApiService) {}

  getAll(request: any): Observable<TaskPagedResponse> {
    const query = this.buildQuery(request);
    return this.api.get<TaskPagedResponse>(this.service, `?${query}`);
  }

  getById(id: number) {
    return this.api.get<BaseResponse<TaskGet>>(this.service, `${id}`);
  }

  create(model: TaskAddEdit) {
    return this.api.post(this.service, '', model);
  }

  update(id: number, model: TaskAddEdit) {
    return this.api.put(this.service, `${id}`, model);
  }

  delete(id: number) {
    return this.api.delete(this.service, `${id}`);
  }

private buildQuery(req: any): string {
  const params: string[] = [];

  params.push(`searchKey=${encodeURIComponent(req.searchKey || '')}`);
  params.push(`PageIndex=${req.pageIndex}`);
  params.push(`PageSize=${req.pageSize}`);
  params.push(`SortColumn=${req.sortColumn}`);
  params.push(`SortDirection=${req.sortDirection}`);

  if (Array.isArray(req.employeeIds) && req.employeeIds.length > 0) {
    req.employeeIds.forEach((id: number) => {
      params.push(`employeeIds=${id}`);
    });
  }

  if (req.statusId) {
    params.push(`statusId=${req.statusId}`);
  }

  return params.join('&');
}


   getAssignedEmployees(taskId: number): Observable<BaseResponse<TaskAssignedEmployee[]>> {
    return this.api.get<BaseResponse<TaskAssignedEmployee[]>>(this.service, `${taskId}/assigned-employees`);
  }

   getTaskRequests(taskId: number): Observable<BaseResponse<TaskRequests>> {
    return this.api.get<BaseResponse<TaskRequests>>(this.service, `${taskId}/requests`);
  }
}

