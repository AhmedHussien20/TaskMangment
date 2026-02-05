import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TaskCommentService } from 'app/core/services/task-comment.service';
import { TaskCommentGetDto } from 'app/core/models/task/task-comment';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MyDatePipe } from 'app/components/utilities/pipline/MyDatePipe';

@Component({
  selector: 'app-task-comments',
  standalone: true,
  imports: [CommonModule, TranslateModule, MyDatePipe],
  templateUrl: './task-comments.component.html',
      styleUrls: ['./task-comments.component.scss']

})
export class TaskCommentsComponent implements OnInit, OnDestroy {
  @Input() taskId!: number;

  comments: (TaskCommentGetDto & { timeLabel?: string })[] = [];
  private sub!: Subscription;

  constructor(
    private refresh: TaskDetailsRefreshService,
    private commentService: TaskCommentService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadComments();
    this.sub = this.refresh.refresh$.subscribe(() => this.loadComments());
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
      next: (res) => {
        if (res.success && res.data) {
          this.comments = res.data.data.map(c => ({
            ...c,
            timeLabel: this.timeAgo(c.createdDate)
          }));
        }
      },
      error: (err) => console.error('Failed to load comments', err)
    });
  }

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



  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
