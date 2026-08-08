import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbNavModule } from '@ng-bootstrap/ng-bootstrap';
import { TaskEmployeesComponent } from '../tabs/employees/task-employees/task-employees.component';
import { TaskBasicInfoComponent } from '../tabs/basic-info/task-basic-info/task-basic-info.component';
import { TranslateModule } from '@ngx-translate/core';
import { TaskWarningsComponent } from '../tabs/warnings/task-warnings/task-warnings.component';
import { TaskPenaltiesComponent } from '../tabs/penalties/task-penalties/task-penalties.component';
import { TaskAuditComponent } from '../tabs/audit/task-audit/task-audit.component';
import { TaskRequestsComponent } from '../tabs/requests/task-requests/task-requests.component';
import { TaskCommentsComponent } from '../tabs/comments/task-comments/task-comments.component';
import { TaskUpdatesComponent } from '../tabs/updates/task-updates/task-updates.component';
import { TaskGet } from 'app/core/models/task/task';

@Component({
  selector: 'app-task-tabs',
  standalone: true,
  imports: [
    CommonModule,
    NgbNavModule,
    TaskEmployeesComponent,
    TaskBasicInfoComponent,
    TaskWarningsComponent,
    TranslateModule,
    TaskPenaltiesComponent,
    TaskAuditComponent,
    TaskRequestsComponent,
    TaskCommentsComponent,
    TaskUpdatesComponent
  ],
  templateUrl: './task-tabs.component.html',
  styleUrls: ['./task-tabs.component.scss']
})
export class TaskTabsComponent implements OnInit, OnChanges {

  @Input() taskId!: number;
  @Input() taskInfo: TaskGet | null = null;
  @Input() createdByMe: boolean = false;
  @Input() readonly = false;
  @Input() canSendPenalty = false;
  @Input() canSendWarning = false;
  @Input() initialTab?: string;
  @Output() updatesViewed = new EventEmitter<void>();

  activeTab = 'basic';

  get pendingRequestsCount(): number {
    return this.taskInfo?.pendingRequestsCount ?? 0;
  }

  get unreadNotificationsCount(): number {
    return this.taskInfo?.unreadNotificationsCount ?? 0;
  }

  ngOnInit(): void {
    if (this.initialTab) {
      this.activeTab = this.initialTab;
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialTab'] && this.initialTab) {
      this.activeTab = this.initialTab;
    }
  }

  onUpdatesViewed(): void {
    this.updatesViewed.emit();
  }
}
