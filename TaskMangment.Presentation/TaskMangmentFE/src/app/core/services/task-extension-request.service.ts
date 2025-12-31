import { Injectable } from "@angular/core";
import {
  TaskExtensionRequestAdd,
  TaskExtensionRequestGet,
  TaskExtensionRequestPagedResponse,
  TaskExtensionReviewDto
} from "../models/task/task-extension-request";
import { ApiService } from "./api.service";
import { BaseResponse } from "app/models/base.response.model";
import { Observable } from "rxjs";

@Injectable({ providedIn: 'root' })
export class TaskExtensionRequestService {

  private readonly service = 'TaskExtensionRequest';

  constructor(private api: ApiService) {}

  getAll(request: any): Observable<TaskExtensionRequestPagedResponse> {
    const query = this.buildQuery(request);
    return this.api.get<TaskExtensionRequestPagedResponse>(this.service, `?${query}`);
  }

  getById(id: number) {
    return this.api.get<BaseResponse<TaskExtensionRequestGet>>(this.service, `${id}`);
  }

  create(taskId: number, model: TaskExtensionRequestAdd) {
    return this.api.post(this.service, `${taskId}`, model);
  }

  review(id: number, dto: TaskExtensionReviewDto) {
    return this.api.patch(this.service,`review/${id}`,dto);
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
