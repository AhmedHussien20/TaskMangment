import { Injectable } from "@angular/core";
import { TaskAddEdit, TaskAssignedEmployee, TaskGet, TaskPagedResponse } from "../models/task/task";
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
    return [
      `searchKey=${req.searchKey || ''}`,
      `PageIndex=${req.pageIndex}`,
      `PageSize=${req.pageSize}`,
      `SortColumn=${req.sortColumn}`,
      `SortDirection=${req.sortDirection}`
    ].join('&');
  }

   getAssignedEmployees(taskId: number): Observable<BaseResponse<TaskAssignedEmployee[]>> {
    return this.api.get<BaseResponse<TaskAssignedEmployee[]>>(this.service, `${taskId}/assigned-employees`);
  }
}

