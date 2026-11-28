import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { NotificationApiService } from 'app/core/services/notification.service';
import { TaskNotification } from 'app/core/models/notification/notification';
import { MyDatePipe } from 'app/components/utilities/pipline/MyDatePipe';

const NOTIFICATION_TYPE_NAMES = [
  'Comments',
  'Penalty',
  'Warning',
  'TaskAssign',
  'TaskUnassign',
  'LeaveRequest',
  'leaveApproved',
  'leaverejected',
  'ExtensionRequest',
  'ExtensionRequestApproved',
  'ExtensionRequestRejected',
  'CloseRequest',
  'CloseRequestApproved',
  'CloseRequestRejected',
  'AchievementPercent'
];

@Component({
  selector: 'app-task-updates',
  standalone: true,
  imports: [CommonModule, TranslateModule, MyDatePipe],
  templateUrl: './task-updates.component.html',
  styleUrls: ['./task-updates.component.scss']
})
export class TaskUpdatesComponent implements OnInit {
  @Input() taskId!: number;
  @Output() updatesViewed = new EventEmitter<void>();

  isLoading = false;
  items: TaskNotification[] = [];

  constructor(private notificationService: NotificationApiService) {}

  ngOnInit(): void {
    this.updatesViewed.emit();
    this.load();
  }

  private load(): void {
    if (!this.taskId) return;

    this.isLoading = true;
    this.notificationService.getByTask(this.taskId).subscribe({
      next: (res) => {
        this.items = res?.data ?? [];
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  typeLabelKey(type: string | number | undefined): string {
    if (typeof type === 'number') {
      return `EMPLOYEE_360.NOTIFICATION_TYPES.${NOTIFICATION_TYPE_NAMES[type] ?? 'Comments'}`;
    }
    if (type == null || type === '') {
      return 'EMPLOYEE_360.NOTIFICATION_TYPES.Comments';
    }
    return `EMPLOYEE_360.NOTIFICATION_TYPES.${type}`;
  }
}
