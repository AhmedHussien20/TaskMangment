import { Injectable } from "@angular/core";
import {
  CloseRequestStatus,
  TaskCloseRequestAdd,
  TaskCloseRequestGet,
  TaskCloseRequestPagedResponse
} from "../models/task/task-close-request";
import { ApiService } from "./api.service";
import { BaseResponse } from "app/models/base.response.model";
import { Observable } from "rxjs";

@Injectable({ providedIn: 'root' })
export class TaskCloseRequestService {

  private readonly service = 'TaskCloseRequest';

  constructor(private api: ApiService) {}

  getAll(request: any): Observable<TaskCloseRequestPagedResponse> {
    const query = this.buildQuery(request);
    return this.api.get<TaskCloseRequestPagedResponse>(this.service, `?${query}`);
  }

  getById(id: number) {
    return this.api.get<BaseResponse<TaskCloseRequestGet>>(this.service, `${id}`);
  }

  create(taskId: number, model: TaskCloseRequestAdd) {
    return this.api.post(this.service, `${taskId}`, model);
  }

  review(id: number, status: CloseRequestStatus) {
      return this.api.patch(this.service,`review/${id}`,status);
    }

  private buildQuery(req: any): string {
    return [
      `TaskId=${req.taskId}`,
      `searchKey=${req.searchKey || ''}`,
      `PageIndex=${req.pageIndex}`,
      `PageSize=${req.pageSize}`,
      `SortColumn=${req.sortColumn}`,
      `SortDirection=${req.sortDirection}`
    ].join('&');
  }
}
