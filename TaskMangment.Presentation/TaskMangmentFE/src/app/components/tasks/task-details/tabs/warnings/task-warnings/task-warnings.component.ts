import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TranslateModule } from '@ngx-translate/core';
import { WarningGetDto} from 'app/core/models/task/task-warning';
import { TaskWarningService } from 'app/core/services/task-warning.service';
import { MyDatePipe } from 'app/components/utilities/pipline/MyDatePipe';

@Component({
  selector: 'app-task-warnings',
  standalone: true,
  imports: [CommonModule, GenericTableComponent, TranslateModule],
    providers: [MyDatePipe], 

  templateUrl: './task-warnings.component.html'
})
export class TaskWarningsComponent implements OnInit, OnDestroy {
  @Input() taskId!: number;
  @Input() readonly = false;

  rows: WarningGetDto[] = [];
  totalItems = 0;

  columns: TableColumn[] = [
     { key: 'issuedAt', label: 'TASK.DATE', type: 'date' },
    { key: 'reason', label: 'TASK.WARNING_RESON' },
    { key: 'issuedEmployeeName', label: 'TASK.WARNED_EMPLOYEE' },
    { key: 'issuedByName', label: 'TASK.WARNED_BY' },
    { key: 'isRead', label: 'TASK.READED', type: 'seen' }


  ];

  private sub!: Subscription;

  constructor(
    private refresh: TaskDetailsRefreshService,
    private warningService: TaskWarningService,
    private myDatePipe: MyDatePipe

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
      sortDirection: 'DESC'
    };

    this.warningService.getAll(request).subscribe({
      next: (res) => {
        this.rows = res.data.data; 
        this.totalItems = res.data.totalCount;
      },
      error: (err) => {console.error('Failed to load warnings', err)
         console.error('Failed to load warnings - Full Error:', err);
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
