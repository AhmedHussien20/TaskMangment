import { Injectable } from "@angular/core";
import {
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

  review(id: number, approved: boolean) {
    return this.api.put(this.service, `review/${id}?approved=${approved}`, {});
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
