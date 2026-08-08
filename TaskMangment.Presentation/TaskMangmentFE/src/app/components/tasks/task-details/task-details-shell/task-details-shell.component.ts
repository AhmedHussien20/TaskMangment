import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
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
import { NotificationApiService } from 'app/core/services/notification.service';
import { Subscription } from 'rxjs';

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
export class TaskDetailsShellComponent implements OnInit, OnChanges, OnDestroy {

  @Input() taskId!: number;
  @Input() createdByMe: boolean = false;
  @Input() initialTab?: string;
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
  isClosing = false;

  private refreshSub?: Subscription;
  /** True after the مستجدات tab was opened (used by list on dismiss). */
  updatesViewed = false;
  private markedSeen = false;

  constructor(
    private modal: NgbModal,
    private refreshService: TaskDetailsRefreshService,
    private taskService: TaskService,
    public modall: NgbActiveModal,
    private auth: AuthService,
    private notificationService: NotificationApiService
  ) { }

  taskInfo: TaskGet | null = null;
  private loadedTaskId: number | null = null;

  ngOnInit(): void {
    if (this.initialTab === 'updates') {
      this.updatesViewed = true;
    }
    this.refreshPermissions();
    this.ensureTaskLoaded();
    this.refreshSub = this.refreshService.refresh$.subscribe(() => {
      if (this.taskId) this.loadTask();
    });
  }

  ngOnDestroy(): void {
    this.refreshSub?.unsubscribe();
    // Escape / backdrop dismiss path
    this.markTaskUpdatesSeen(false);
  }

  onUpdatesViewed(): void {
    this.updatesViewed = true;
  }

  closeDetails(): void {
    if (this.isClosing) return;

    if (!this.shouldMarkUpdatesSeen()) {
      this.modall.dismiss();
      return;
    }

    this.isClosing = true;
    this.markTaskUpdatesSeen(true);
  }

  private shouldMarkUpdatesSeen(): boolean {
    return this.updatesViewed
      && !this.markedSeen
      && !!this.taskId
      && (this.taskInfo?.unreadNotificationsCount ?? 0) > 0;
  }

  private markTaskUpdatesSeen(closeModal: boolean): void {
    if (!this.shouldMarkUpdatesSeen()) {
      if (closeModal) this.modall.dismiss();
      return;
    }

    this.markedSeen = true;
    this.notificationService.markAsReadByTask(this.taskId).subscribe({
      next: () => {
        this.notificationService.notifyUnreadChanged();
        if (closeModal) {
          this.modall.close({ markedSeen: true, taskId: this.taskId });
        }
      },
      error: () => {
        this.markedSeen = false;
        this.isClosing = false;
        if (closeModal) this.modall.dismiss();
      }
    });
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['taskId']) {
      this.ensureTaskLoaded();
    }
    if (changes['createdByMe']) {
      this.refreshPermissions();
    }
  }

  private ensureTaskLoaded(): void {
    if (!this.taskId || this.loadedTaskId === this.taskId) return;
    this.loadedTaskId = this.taskId;
    this.loadTask();
  }

  private refreshPermissions(): void {
    const isAssignee = this.isCurrentUserAssignee();
    const isCreatorSide = this.createdByMe;
    const isLiteralOwner = !!this.taskInfo?.isCreatorOrAssigner;

    // Employee-side actions (close/extend request) — not for creator-side viewers.
    this.canComment =
      this.auth.hasPermission(Permissions.COMMENT_TASK) || isCreatorSide;
    this.canCloseRequest =
      !isCreatorSide && this.auth.hasPermission(Permissions.REQUEST_TASK_CLOSE);
    this.canExtendRequest =
      !isCreatorSide && this.auth.hasPermission(Permissions.REQUEST_DUE_DATE_EXTENSION);

    // Warn/penalty: literal owner always; scope-only viewers need ISSUE_* permission.
    this.canSendWarning =
      !isAssignee && (isLiteralOwner || (isCreatorSide && this.auth.hasPermission(Permissions.ISSUE_WARNING)));
    this.canSendPenalty =
      !isAssignee && (isLiteralOwner || (isCreatorSide && this.auth.hasPermission(Permissions.ISSUE_PENALTY)));
    // Percentage has no dedicated permission — literal owner only.
    this.canSetPercentage = isLiteralOwner && !isAssignee;
    this.canReviewRequests = isCreatorSide && this.auth.hasAnyPermission(
      Permissions.APPROVE_TASK_REQUEST,
      Permissions.REJECT_TASK_REQUEST
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
