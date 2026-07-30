import { Component, Input, OnInit, OnDestroy, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TranslateModule } from '@ngx-translate/core';
import { DiscountGetDto } from 'app/core/models/task/task-penalty';
import { TaskPenaltyService } from 'app/core/services/task-penalty.service';
import { AuthService } from 'app/core/services/auth.service';
import { Permissions } from 'app/core/constants/permissions';

@Component({
  selector: 'app-task-penalties',
  standalone: true,
  imports: [CommonModule, GenericTableComponent, TranslateModule],
  templateUrl: './task-penalties.component.html'
})
export class TaskPenaltiesComponent implements OnInit, OnDestroy, OnChanges {

  @Input() taskId!: number;
  @Input() readonly = false;
  @Input() canSendPenalty = false;

  canAdd = false;
  canEdit = false;
  canDelete = false;

  rows: DiscountGetDto[] = [];
  totalItems = 0;
  page = 1;
  entries = 10;

  columns: TableColumn[] = [
    { key: 'violationDate', label: 'TASK.PENALTY_DATE', type: 'date' },
    { key: 'reason', label: 'TASK.PENALTY_REASON' },
    {
      key: 'amount',
      label: 'TASK.PENALTY_AMOUNT_LABEL',
      type: 'custom'
    },
    { key: 'employeeName', label: 'TASK.PENALTY_EMPLOYEE_NAME' },
{
  key: 'autoDiscount',
  label: 'TASK.PENALTY_TYPE',
  type: 'badge',
  badgeMap: {
    true: { text: 'TASK.STATUS_AUTODISCOUNT', class: 'bg-warning' },
    false: { text: 'TASK.STATUS_DISCOUNT', class: 'bg-success' }
  }
},
    { key: 'isRead', label: 'TASK.READED', type: 'seen' }
  ];

  private sub!: Subscription;

  constructor(
    private refresh: TaskDetailsRefreshService,
    private penaltyService: TaskPenaltyService,
    private auth: AuthService
  ) { }

  ngOnInit(): void {
    this.refreshActionFlags();
    this.loadPenalties();
    this.sub = this.refresh.refresh$.subscribe(() => this.loadPenalties());
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['canSendPenalty'] || changes['readonly']) {
      this.refreshActionFlags();
    }
  }

  private refreshActionFlags(): void {
    this.canAdd = !this.readonly && this.canSendPenalty;
    this.canEdit = !this.readonly && this.canSendPenalty;
    this.canDelete = !this.readonly && this.auth.hasPermission(Permissions.DELETE_PENALTY);
  }

  loadPenalties(pageIndex = this.page, pageSize = this.entries) {
    const request = {
      taskId: this.taskId,
      searchKey: '',
      pageIndex,
      pageSize,
      sortColumn: 'CreatedDate',
      sortDirection: 'DESC'
    };

    this.penaltyService.getAll(request).subscribe({
      next: (res) => {
        this.rows = res.data.data;
        this.totalItems = res.data.totalCount;
        this.page = res.data.pageIndex ?? pageIndex;
      },
      error: (err) => {
        console.error('Failed to load penalties', err);
      }
    });
  }

  onPageChange(page: number): void {
    this.page = page;
    this.loadPenalties(page, this.entries);
  }

  onEntriesChange(entries: number): void {
    this.entries = entries;
    this.page = 1;
    this.loadPenalties(1, entries);
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
