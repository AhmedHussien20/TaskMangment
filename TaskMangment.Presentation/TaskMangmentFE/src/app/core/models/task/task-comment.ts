// app/core/models/task/task-comment.ts
export interface TaskCommentAddEditDto {
  commentText: string;
  file?: File;
}

export interface TaskCommentGetDto {
  id: number;
  taskId: number;
  taskTitle: string;
  commentText: string;
  employeeName: string;
  createdDate: string;
  attachmentCount: number;
}


export interface TaskCommentPagedResponse {
  data: TaskCommentGetDto[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}