import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TranslateModule } from '@ngx-translate/core';
import { WarningGetDto} from 'app/core/models/task/task-warning';
import { TaskWarningService } from 'app/core/services/task-warning.service';

@Component({
  selector: 'app-task-warnings',
  standalone: true,
  imports: [CommonModule, GenericTableComponent, TranslateModule],
  templateUrl: './task-warnings.component.html'
})
export class TaskWarningsComponent implements OnInit, OnDestroy {
  @Input() taskId!: number;
  @Input() readonly = false;

  rows: WarningGetDto[] = [];
  totalItems = 0;

  columns = [
    { key: 'issuedAt', label: 'DATE' },
    { key: 'reason', label: 'WARNING.REASON' },
    { key: 'issuedEmployeeName', label: 'WARNING.WARNED_EMPLOYEE' },
    { key: 'issuedByName', label: 'WARNING.ISSUED_BY' }
  ];

  private sub!: Subscription;

  constructor(
    private refresh: TaskDetailsRefreshService,
    private warningService: TaskWarningService
  ) {}

  ngOnInit(): void {
    this.loadWarnings();

    this.sub = this.refresh.refresh$.subscribe(() => this.loadWarnings());
  }

  loadWarnings(pageIndex = 1, pageSize = 10) {
    const request = {
      taskId: this.taskId,
      searchKey: '',
      pageIndex,
      pageSize,
      sortColumn: 'IssuedAt',
      sortDirection: 'desc'
    };

    this.warningService.getAll(request).subscribe({
      next: (res) => {
        this.rows = res.data.data; 
        this.totalItems = res.data.totalCount;
      },
      error: (err) => {console.error('Failed to load warnings', err)
         console.error('Failed to load warnings - Full Error:', err);
  console.error('Error Status:', err.status);
  console.error('Error Message:', err.message);
  console.error('Error Body:', err.error); // هذا قد يحتوي على رسالة الخطأ من السيرفر
      }
      
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
