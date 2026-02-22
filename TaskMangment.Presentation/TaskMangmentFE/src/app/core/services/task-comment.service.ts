// app/core/services/task-comment.service.ts
import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { Observable } from 'rxjs';
import { 
  AttachmentVm,
  TaskCommentAddEditDto, 
  TaskCommentGetDto, 
  TaskCommentPagedResponse 
} from '../models/task/task-comment';
import { BaseResponse } from 'app/models/base.response.model';
import { ApiResponse } from '../models/event/calendar';

@Injectable({ providedIn: 'root' })
export class TaskCommentService {
  private readonly service = 'TaskComment';

  constructor(private api: ApiService) {}

  getAll(request: any): Observable<BaseResponse<TaskCommentPagedResponse>> {
    const query = this.buildQuery(request);
    return this.api.get<BaseResponse<TaskCommentPagedResponse>>(this.service, `?${query}`);
  }

  getById(id: number): Observable<BaseResponse<TaskCommentGetDto>> {
    return this.api.get<BaseResponse<TaskCommentGetDto>>(this.service, `${id}`);
  }

  create(taskId: number, formData: FormData): Observable<BaseResponse<TaskCommentGetDto>> {
  return this.api.post<BaseResponse<TaskCommentGetDto>>(this.service, `${taskId}`, formData);
}


  update(id: number, model: TaskCommentAddEditDto): Observable<BaseResponse<TaskCommentGetDto>> {
    const formData = new FormData();
    formData.append('commentText', model.commentText);
    
    if (model.files && model.files.length > 0) {
      model.files.forEach(file => {
        formData.append('files', file);
      });
    }
    
    return this.api.put<BaseResponse<TaskCommentGetDto>>(this.service, `${id}`, formData);
  }

  delete(id: number): Observable<BaseResponse<boolean>> {
    return this.api.delete<BaseResponse<boolean>>(this.service, `${id}`);
  }

  getCommentAttachments(commentId: number): Observable<BaseResponse<AttachmentVm[]>> {
  return this.api.get<BaseResponse<AttachmentVm[]>>(this.service, `comments/${commentId}/attachments`);
}

downloadAttachment(attachmentId: number): Observable<Blob> { 
  return this.api.get(this.service, `comments/${attachmentId}/download`, {
    responseType: 'blob'
  });
}


  private buildQuery(req: any): string {
    const params = new URLSearchParams();
    
    params.append('taskId', req.taskId?.toString() || '');
    params.append('searchKey', req.searchKey || '');
    params.append('pageIndex', req.pageIndex?.toString() || '1');
    params.append('pageSize', req.pageSize?.toString() || '10');
    params.append('sortColumn', req.sortColumn || 'createdDate');
    params.append('sortDirection', req.sortDirection || 'desc');
    
    return params.toString();
  }
}