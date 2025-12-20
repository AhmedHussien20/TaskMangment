import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component'; 
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TranslateModule } from '@ngx-translate/core';
import { DiscountListDto } from 'app/core/models/task/task-penalty';
import { TaskPenaltyService } from 'app/core/services/task-penalty.service';

@Component({
  selector: 'app-task-penalties',
  standalone: true,
  imports: [CommonModule, GenericTableComponent, TranslateModule],
  templateUrl: './task-penalties.component.html'
})
export class TaskPenaltiesComponent implements OnInit, OnDestroy {

  @Input() taskId!: number;
  @Input() readonly = false;

  rows: DiscountListDto[] = [];
  totalItems = 0;

  columns = [
    { key: 'createdAt', label: 'TASK.DATE' },
    { key: 'reason', label: 'TASK.PENALTY_REASON'},
    { key: 'amount', label: 'TASK.PENALTY_AMOUNT_LABEL' },
    { key: 'employeeName', label: 'TASK.PENALTY_EMPLOYEE_NAME' }
  ];

  private sub!: Subscription;

  constructor(
    private refresh: TaskDetailsRefreshService,
    private penaltyService: TaskPenaltyService
  ) {}

  ngOnInit(): void {
    this.loadPenalties();
    this.sub = this.refresh.refresh$.subscribe(() => this.loadPenalties());
  }

  loadPenalties(pageIndex = 1, pageSize = 10) {
    const request = {
      taskId: this.taskId,
      searchKey: '',
      pageIndex,
      pageSize,
      sortColumn: 'CreatedDate',
      sortDirection: 'desc'
    };

    this.penaltyService.getAll(request).subscribe({
      next: (res) => {
        this.rows = res.data.data;
        this.totalItems = res.data.totalCount;
      },
      error: (err) => {
        console.error('Failed to load penalties', err);
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
