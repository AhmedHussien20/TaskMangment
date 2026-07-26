import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { TaskTabsComponent } from '../task-tabs/task-tabs.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { CommentModalComponent } from '../actions/comment/comment-modal/comment-modal.component';
import { NgbActiveModal, NgbModal, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { TaskDetailsRefreshService } from '../task-details-refresh.service';
import { WarningModalComponent } from '../actions/warning/warning-modal/warning-modal.component';
import { PenaltyModalComponent } from '../actions/penalty/penalty-modal/penalty-modal.component';
import { CloseRequestModalComponent } from '../actions/close-request/close-request-modal/close-request-modal.component';
import { ExtendRequestComponent } from '../actions/extend-request/extend-request.component';
import { TaskService } from 'app/core/services/task.service';
import { TaskGet } from 'app/core/models/task/task';
import { AuthService } from 'app/core/services/auth.service';
import { CommonModule } from '@angular/common';
import { PercentageModalComponent } from '../actions/task-percentage/percentage-modal/percentage-modal.component';
import { Permissions } from 'app/core/constants/permissions';

@Component({
  selector: 'app-task-details-shell',
  standalone: true,
  templateUrl: './task-details-shell.component.html',
  imports: [
    TaskTabsComponent,
    PageHeaderComponent,
    TranslateModule,
    NgbTooltipModule,
    CommonModule
  ]
})
export class TaskDetailsShellComponent implements OnInit, OnChanges {

  @Input() taskId!: number;
  @Input() createdByMe: boolean = false;
  requireUploadFile = false;

  readonly = false;

  /** Permission-driven action visibility (hide when no permission). */
  canComment = false;
  canCloseRequest = false;
  canExtendRequest = false;
  canSendWarning = false;
  canSendPenalty = false;
  canSetPercentage = false;
  canReviewRequests = false;

  constructor(
    private modal: NgbModal,
    private refreshService: TaskDetailsRefreshService,
    private taskService: TaskService,
    public modall: NgbActiveModal,
    private auth: AuthService
  ) { }

  taskInfo: TaskGet | null = null;

  ngOnInit(): void {
    this.refreshPermissions();
    if (this.taskId) {
      this.loadTask();
    }
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['taskId'] && this.taskId) {
      this.loadTask();
    }
    if (changes['createdByMe']) {
      this.refreshPermissions();
    }
  }

  private refreshPermissions(): void {
    const isAssignee = this.isCurrentUserAssignee();

    this.canComment = this.auth.hasPermission(Permissions.COMMENT_TASK);
    this.canCloseRequest = this.auth.hasPermission(Permissions.CLOSE_TASK_EMPLOYEE);
    this.canExtendRequest = this.auth.hasPermission(Permissions.SUBMIT_DUE_DATE);
    this.canSendWarning =
      this.auth.hasPermission(Permissions.SEND_WARNING) && !isAssignee;
    this.canSendPenalty =
      this.auth.hasPermission(Permissions.SEND_PENALTY) && !isAssignee;
    this.canSetPercentage =
      (this.createdByMe || this.auth.hasPermission(Permissions.UPDATE_TASK)) && !isAssignee;
    this.canReviewRequests = this.auth.hasAnyPermission(
      Permissions.APPROVE_CLOSE_EXTEND,
      Permissions.REJECT_CLOSE_EXTEND,
      Permissions.EXTEND_DUE_DATE
    );
  }

  private isCurrentUserAssignee(): boolean {
    const userId = this.auth.getUser()?.userId ?? this.auth.getCurrentUser()?.userId;
    if (!userId || !this.taskInfo?.assignEmployee?.length) return false;
    return this.taskInfo.assignEmployee.some(e => e.id === userId);
  }

  loadTask() {
    this.taskService.getById(this.taskId).subscribe({
      next: res => {
        this.taskInfo = res.data;
        this.requireUploadFile = this.taskInfo.requireUploadFile;
        this.createdByMe = !!this.taskInfo.createdByMe;
        this.refreshPermissions();
      },
      error: err => {
        console.error('Failed to load task', err);
        this.modall.dismiss();
      }
    });
  }

  get isTaskClosed(): boolean {
    const status = this.taskInfo?.status;
    return status === 5
      || status === 4
      || status === 3;
  }

  openAction(action: string) {
    if (this.isTaskClosed) return;

    if (action === 'comment') {
      if (!this.canComment) return;
      const ref = this.modal.open(CommentModalComponent, {
        size: 'lg',
        backdrop: 'static'
      });

      ref.componentInstance.taskId = this.taskId;
      ref.componentInstance.requireUploadFile = this.createdByMe ? false : (this.taskInfo?.requireUploadFile ?? false);
      ref.componentInstance.createdByMe = this.createdByMe;

      ref.result.then(
        (success) => {
          if (success) {
            this.refreshService.trigger('comment');
          }
        },
        () => { }
      );
    }
    if (action === 'warning') {
      if (!this.canSendWarning) return;
      const ref = this.modal.open(WarningModalComponent, { size: 'lg' });
      ref.componentInstance.taskId = this.taskId;

      ref.result.then(
        ok => ok && this.refreshService.trigger('warning'),
        () => { }
      );
    }

    if (action === 'percent') {
      if (!this.canSetPercentage) return;
      const ref = this.modal.open(PercentageModalComponent, {
        size: 'lg',
        backdrop: 'static'
      });

      ref.componentInstance.taskId = this.taskId;

      ref.result.then(
        (success) => {
          if (success) {
            this.refreshService.trigger('percent');
          }
        },
        () => { }
      );
    }

    if (action === 'penalty') {
      if (!this.canSendPenalty) return;
      const ref = this.modal.open(PenaltyModalComponent, { size: 'lg' });
      ref.componentInstance.taskId = this.taskId;

      ref.result.then(
        ok => ok && this.refreshService.trigger('penalty'),
        () => { }
      );
    }
    if (action === 'extend') {
      if (!this.canExtendRequest) return;
      const ref = this.modal.open(ExtendRequestComponent, {
        size: 'lg',
        backdrop: 'static'
      });

      ref.componentInstance.taskId = this.taskId;
      return;
    }
    if (action === 'close') {
      if (!this.canCloseRequest) return;
      const ref = this.modal.open(CloseRequestModalComponent, {
        size: 'lg',
        backdrop: 'static'
      });

      ref.componentInstance.taskId = this.taskId;

      ref.result.then(
        success => {
          if (success) {
            this.refreshService.trigger('close-request');
          }
        },
        () => { }
      );
    }
  }
}
