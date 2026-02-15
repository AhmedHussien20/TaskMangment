import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TaskCommentService } from 'app/core/services/task-comment.service';
import { TaskCommentGetDto } from 'app/core/models/task/task-comment';
import { MyDatePipe } from 'app/components/utilities/pipline/MyDatePipe';

export interface AttachmentVm {
  id: number;
  fileName: string;
  url: string;
  urlDownload: string;
  contentType?: string;
  size?: number;
  uploadedAt: string;
}

type CommentRow = TaskCommentGetDto & { timeLabel?: string };

@Component({
  selector: 'app-task-comments',
  standalone: true,
  imports: [CommonModule, TranslateModule, MyDatePipe],
  templateUrl: './task-comments.component.html',
  styleUrls: ['./task-comments.component.scss']
})
export class TaskCommentsComponent implements OnInit, OnDestroy {
  @Input() taskId!: number;

  comments: CommentRow[] = [];
  private sub?: Subscription;

  // attachments state
  openAttachmentsForCommentId?: number;
  attachmentsMap: Record<number, AttachmentVm[]> = {};
  loadingAttachments: Record<number, boolean> = {};

  constructor(
    private refresh: TaskDetailsRefreshService,
    private commentService: TaskCommentService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadComments();
    this.sub = this.refresh.refresh$.subscribe(() => this.loadComments());
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  loadComments(): void {
    const request = {
      taskId: this.taskId,
      searchKey: '',
      pageIndex: 1,
      pageSize: 10,
      sortColumn: 'CreatedDate',
      sortDirection: 'DESC'
    };

    this.commentService.getAll(request).subscribe({
      next: (res: any) => {
        if (res?.success && res?.data?.data) {
          this.comments = res.data.data.map((c: TaskCommentGetDto) => ({
            ...c,
            timeLabel: this.timeAgo(c.createdDate)
          }));

          // لو comment اتقفل attachments panel قبل كده، نقفله لو اختفى من الصفحة
          if (this.openAttachmentsForCommentId) {
            const exists = this.comments.some(x => x.id === this.openAttachmentsForCommentId);
            if (!exists) this.openAttachmentsForCommentId = undefined;
          }
        } else {
          this.comments = [];
        }
      },
      error: (err: any) => console.error('Failed to load comments', err)
    });
  }

  // ===== Attachments UI =====

  toggleAttachments(commentId: number): void {
    // close if open
    if (this.openAttachmentsForCommentId === commentId) {
      this.openAttachmentsForCommentId = undefined;
      return;
    }

    this.openAttachmentsForCommentId = commentId;

    // already loaded
    if (this.attachmentsMap[commentId]) return;

    this.loadingAttachments[commentId] = true;

    this.commentService.getCommentAttachments(commentId).subscribe({
      next: (res: any) => {
        this.attachmentsMap[commentId] = (res?.data ?? []) as AttachmentVm[];
        this.loadingAttachments[commentId] = false;
      },
      error: (err: any) => {
        console.error('Failed to load attachments', err);
        this.attachmentsMap[commentId] = [];
        this.loadingAttachments[commentId] = false;
      }
    });
  }

 downloadAttachment(att: AttachmentVm): void {
  const url = this.commentService.downloadAttachment(att.id);
}




  openAttachment(att: AttachmentVm): void {
    window.open(att.url, '_blank');
  }

  // ===== Time Ago =====

  timeAgo(dateString: string): string {
    if (!dateString) return '';

    const date = new Date(dateString + 'Z');
    const now = new Date();

    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMins / 60);
    const diffDays = Math.floor(diffHours / 24);

    if (diffDays === 0) {
      if (diffHours > 0)
        return this.translate.instant('TASK.HOURS_AGO', { value: diffHours });
      if (diffMins > 0)
        return this.translate.instant('TASK.MINUTES_AGO', { value: diffMins });
      return this.translate.instant('TASK.NOW');
    }

    return this.translate.instant('TASK.DAYS_AGO', { value: diffDays });
  }
}
