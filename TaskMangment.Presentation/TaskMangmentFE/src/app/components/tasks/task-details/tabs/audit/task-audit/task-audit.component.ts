import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs'; 
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TranslateModule } from '@ngx-translate/core';
import { TaskService } from 'app/core/services/task.service';
import { MyDatePipe } from 'app/components/utilities/pipline/MyDatePipe';

@Component({
  selector: 'app-task-audit',
  standalone: true,
  imports: [CommonModule, TranslateModule, MyDatePipe],
  templateUrl: './task-audit.component.html'
})
export class TaskAuditComponent implements OnInit, OnDestroy {

  @Input() taskId!: number;

  summary: any = null;
  private sub!: Subscription;
  isLoading = false;

  constructor(
    private taskService: TaskService,
    private refresh: TaskDetailsRefreshService
  ) {}

  ngOnInit(): void {
    this.loadAudit();

    this.sub = this.refresh.refresh$
      .subscribe(() => this.loadAudit());
  }

  loadAudit(): void {
    if (!this.taskId) return;
    
    this.isLoading = true;
    this.taskService.getTaskActivitySummary(this.taskId).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.summary = response.data;
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to load activity summary:', err);
        this.isLoading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}