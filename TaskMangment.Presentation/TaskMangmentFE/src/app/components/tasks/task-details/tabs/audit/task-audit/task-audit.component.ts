import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs'; 
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TranslateModule } from '@ngx-translate/core';

interface AuditItem {
  id: number;
  type: 'comment' | 'warning' | 'penalty' | 'close' | 'reopen';
  message: string;
  user: string;
  date: string; // ISO
}

@Component({
  selector: 'app-task-audit',
  standalone: true,
  imports: [CommonModule,TranslateModule],
  templateUrl: './task-audit.component.html'
})
export class TaskAuditComponent implements OnInit, OnDestroy {

  @Input() taskId!: number;

  items: AuditItem[] = [];
  private sub!: Subscription;

  constructor(private refresh: TaskDetailsRefreshService) {}

  ngOnInit(): void {
    this.loadAudit();

    this.sub = this.refresh.refresh$
      .subscribe(() => this.loadAudit());
  }

  loadAudit(): void {
    // TODO: Replace with API
    this.items = [
      {
        id: 1,
        type: 'comment',
        message: 'Added a comment',
        user: 'Ahmed',
        date: '2025-01-12T10:30:00'
      },
      {
        id: 2,
        type: 'warning',
        message: 'Late submission warning',
        user: 'Manager',
        date: '2025-01-11T09:15:00'
      },
      {
        id: 3,
        type: 'penalty',
        message: 'Penalty 500 EGP applied',
        user: 'Manager',
        date: '2025-01-10T14:00:00'
      }
    ];
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  icon(type: AuditItem['type']): string {
    switch (type) {
      case 'comment': return 'bi bi-chat-dots text-primary';
      case 'warning': return 'bi bi-exclamation-triangle text-warning';
      case 'penalty': return 'bi bi-cash-coin text-danger';
      case 'close': return 'bi bi-x-circle text-secondary';
      case 'reopen': return 'bi bi-arrow-counterclockwise text-success';
      default: return 'bi bi-info-circle';
    }
  }
}
