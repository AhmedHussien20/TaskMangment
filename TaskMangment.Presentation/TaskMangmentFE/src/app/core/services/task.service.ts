import { Injectable } from "@angular/core";
import { TaskAddEdit, TaskAssignedEmployee, TaskGet, TaskPagedResponse, TaskRequests } from "../models/task/task";
import { ApiService } from "./api.service";
import { BaseResponse } from 'app/models/base.response.model'; 
import { Observable } from "rxjs";
import { TaskActivitySummary } from "../models/task/task-activity-summary";

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
    console.log('Creating task with model:', model);
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

  // ✅ employeeIds (existing)
  if (Array.isArray(req.employeeIds) && req.employeeIds.length > 0) {
    req.employeeIds.forEach((id: number) => {
      params.push(`employeeIds=${id}`);
    });
  }

  // ✅ statusId (existing)
  if (req.statusId) {
    params.push(`statusId=${req.statusId}`);
  }

  // ==========================
  // ✅ NEW FILTERS
  // ==========================

  // direction: 1 Incoming, 2 Outgoing
  if (req.direction) {
    params.push(`direction=${req.direction}`);
  }

  // targetEmployeeId
  if (req.targetEmployeeId) {
    params.push(`targetEmployeeId=${req.targetEmployeeId}`);
  }

  // priorityId
  if (req.priorityId) {
    params.push(`priorityId=${req.priorityId}`);
  }

  // createdFrom / createdTo
  if (req.createdFrom) {
    params.push(`createdFrom=${encodeURIComponent(req.createdFrom)}`);
  }

  if (req.createdTo) {
    params.push(`createdTo=${encodeURIComponent(req.createdTo)}`);
  }

  // dueFrom / dueTo
  if (req.dueFrom) {
    params.push(`dueFrom=${encodeURIComponent(req.dueFrom)}`);
  }

  if (req.dueTo) {
    params.push(`dueTo=${encodeURIComponent(req.dueTo)}`);
  }

  if (req.viewScopedTasks === 'scoped' || req.viewScopedTasks === true || req.viewScopedTasks === 1) {
    params.push('viewScopedTasks=true');
  }

  return params.join('&');
}


   getAssignedEmployees(taskId: number): Observable<BaseResponse<TaskAssignedEmployee[]>> {
    return this.api.get<BaseResponse<TaskAssignedEmployee[]>>(this.service, `${taskId}/assigned-employees`);
  }

   getTaskRequests(taskId: number): Observable<BaseResponse<TaskRequests>> {
    return this.api.get<BaseResponse<TaskRequests>>(this.service, `${taskId}/requests`);
  }

  getTaskActivitySummary(taskId: number) {
  return this.api.get<BaseResponse<TaskActivitySummary>>(this.service,`${taskId}/activity-summary`);
  }

  archiveClosed(taskIds: number[]): Observable<BaseResponse<boolean>> {
  return this.api.post<BaseResponse<boolean>>(this.service, 'archive-closed', taskIds);
}

}

