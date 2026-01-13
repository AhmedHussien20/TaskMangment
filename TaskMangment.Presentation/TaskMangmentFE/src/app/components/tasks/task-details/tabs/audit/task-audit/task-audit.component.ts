import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs'; 
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TaskService } from 'app/core/services/task.service';
import { MyDatePipe } from 'app/components/utilities/pipline/MyDatePipe';

interface AuditItem {
  id: number;
  type: 'comment' | 'warning' | 'penalty' | 'extension' | 'close' | 'percentage';
  message: string;
  user: string;
  date: string;
}

@Component({
  selector: 'app-task-audit',
  standalone: true,
  imports: [CommonModule, TranslateModule, MyDatePipe],
  templateUrl: './task-audit.component.html'
})
export class TaskAuditComponent implements OnInit, OnDestroy {

  @Input() taskId!: number;

  items: AuditItem[] = [];
  
  commentItems: AuditItem[] = [];
  warningItems: AuditItem[] = [];
  penaltyItems: AuditItem[] = [];
  extensionItems: AuditItem[] = [];
  closeItems: AuditItem[] = [];
  percentageItems: AuditItem[] = [];
  
  counts: Record<string, number> = {};
  private sub!: Subscription;
  isLoading = false;

  constructor(
    private taskService: TaskService,
    private refresh: TaskDetailsRefreshService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadAudit();

    this.sub = this.refresh.refresh$.subscribe(() => this.loadAudit());
  }

  loadAudit(): void {
    if (!this.taskId) return;

    this.isLoading = true;
    this.taskService.getTaskActivitySummary(this.taskId).subscribe({
      next: (res) => {
        this.items = [];
        
        this.commentItems = [];
        this.warningItems = [];
        this.penaltyItems = [];
        this.extensionItems = [];
        this.closeItems = [];
        this.percentageItems = [];
        
        this.counts = {
          comment: 0,
          warning: 0,
          penalty: 0,
          extension: 0,
          close: 0,
          percentage: 0
        };

        if (res.success && res.data) {
          const summary = res.data;

          if (summary.lastComment) {
            const commentItem: AuditItem = {
              id: summary.lastComment.id,
              type: 'comment',
              message: summary.lastComment.commentText,
              user: summary.lastComment.employeeName || '',
              date: summary.lastComment.createdDate
            };
            this.items.push(commentItem);
            this.commentItems.push(commentItem);
            this.counts['comment'] = summary.commentsCount || 0;
          }

          // ===== Warnings =====
          if (summary.lastWarning) {
            const warningItem: AuditItem = {
              id: summary.lastWarning.id,
              type: 'warning',
              message: summary.lastWarning.reason,
              user: summary.lastWarning.issuedByName || '',
              date: summary.lastWarning.issuedAt
            };
            this.items.push(warningItem);
            this.warningItems.push(warningItem);
            this.counts['warning'] = summary.warningsCount || 0;
          }

          // ===== Penalties =====
          if (summary.lastPenalty) {
            const penaltyItem: AuditItem = {
              id: summary.lastPenalty.taskId,
              type: 'penalty',
              message: `${summary.lastPenalty.amount} - ${summary.lastPenalty.reason}`,
              user: summary.lastPenalty.employeeName || '',
              date: summary.lastPenalty.createdDate
            };
            this.items.push(penaltyItem);
            this.penaltyItems.push(penaltyItem);
            this.counts['penalty'] = summary.penaltysCount || 0;
          }

          // ===== Extension Requests =====
          if (summary.lastExtensionRequest) {
            const extensionItem: AuditItem = {
              id: summary.lastExtensionRequest.id,
              type: 'extension',
              message: summary.lastExtensionRequest.reason +
                       (summary.lastExtensionRequest.extendRequestText ? ` (${summary.lastExtensionRequest.extendRequestText})` : ''),
              user: summary.lastExtensionRequest.requestedByName || '',
              date: summary.lastExtensionRequest.requestedAt
            };
            this.items.push(extensionItem);
            this.extensionItems.push(extensionItem);
            this.counts['extension'] = summary.extensionRequestsCount || 0;
          }

          // ===== Close Requests =====
          if (summary.lastCloseRequest) {
            const closeItem: AuditItem = {
              id: summary.lastCloseRequest.id,
              type: 'close',
              message: summary.lastCloseRequest.message +
                       (summary.lastCloseRequest.closeRequestText ? ` (${summary.lastCloseRequest.closeRequestText})` : ''),
              user: summary.lastCloseRequest.requestedByName || '',
              date: summary.lastCloseRequest.requestedAt
            };
            this.items.push(closeItem);
            this.closeItems.push(closeItem);
            this.counts['close'] = summary.closeRequestsCount || 0;
          }

          // ===== Percentage =====
          if (summary.lastPercentage) {
            const percentageItem: AuditItem = {
              id: summary.lastPercentage.id,
              type: 'percentage',
message: this.translate.instant('TASK.ACHIEVEMENT') + ': ' + summary.lastPercentage.achievementPercent,
              user: summary.lastPercentage.employeeName || '',
              date: summary.lastPercentage.createdDate
            };
            this.items.push(percentageItem);
            this.percentageItems.push(percentageItem);
            this.counts['percentage'] = summary.percentageCount || 0;
          }

          this.items.sort((a, b) => new Date(b.date).getTime() - new Date(a.date).getTime());
        }

        this.isLoading = false;
      },
      error: (err) => {
        console.error(err);
        this.isLoading = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  icon(type: AuditItem['type']): string {
    switch (type) {
      case 'comment': return 'bi bi-chat-dots text-primary';
      case 'warning': return 'bi bi-exclamation-triangle text-warning';
      case 'penalty': return 'bi bi-cash-coin text-danger';
      case 'extension': return 'bi bi-clock-history text-info';
      case 'close': return 'bi bi-x-circle text-secondary';
      case 'percentage': return 'bi bi-bar-chart-line text-success';
      default: return 'bi bi-info-circle';
    }
  }
}