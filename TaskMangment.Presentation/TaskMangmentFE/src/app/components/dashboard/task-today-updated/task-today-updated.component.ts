import { Component, Input, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { DashboardService } from 'app/core/services/dashboar.service';
import { InProgressUpdatedTodayDto } from 'app/core/models/dashboard/dashboard.model';
import { TaskDetailsShellComponent } from 'app/components/tasks/task-details/task-details-shell/task-details-shell.component';
import { SearchCriteria } from 'app/models/search-criteria.model';

@Component({
  selector: 'app-task-today-updated',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbPaginationModule, GenericTableComponent, TranslateModule],
  templateUrl: './task-today-updated.component.html',
  styleUrl: './task-today-updated.component.scss'
})
export class TaskTodayUpdatedComponent {

  @Input() currentPeriod: any = null;
  @Input() currentBranchId: any = null;

  title = 'DASHBOARD.TODAY_INPROGRESS_TASKS';

  columns: TableColumn[] = [
    { key: 'title', label: 'TASK.TITLE' },
    { key: 'updatedBy', label: 'TASK.UPDATED_BY' },
    { key: 'dueDate', label: 'TASK.DUE_DATE', type: 'date' },
    { key: 'updatedAt', label: 'TASK.UPDATED_AT', type: 'dateTime' }
  ];

  rows: InProgressUpdatedTodayDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  isLoading = false;

  searchCriteria: SearchCriteria = {
    searchKey: '',
    pageIndex: this.page,
    pageSize: this.entries,
    sortColumn: 'UpdatedAt',
    sortDirection: 'DESC',
    filterTypes: {
      searchKey: 'text'
    }
  };

  labels = { searchKey: 'TASK.SEARCH' };

  constructor(
    private dashboardService: DashboardService,
    private modalService: NgbModal
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnChanges(changes: SimpleChanges): void {
  const periodChanged =
    changes['currentPeriod'] && !changes['currentPeriod'].firstChange;

  const branchChanged =
    changes['currentBranchId'] && !changes['currentBranchId'].firstChange;

  if (periodChanged || branchChanged) {
    this.page = 1;
    this.searchCriteria.pageIndex = 1;
    this.loadData();
  }
}


  loadData() {
    if (!this.currentPeriod) return;

    this.isLoading = true;

    const req = {
      ...this.searchCriteria,
      period: this.currentPeriod,
      branchId: this.currentBranchId
    };
 console.log('Request being sent:', req);
    this.dashboardService.getAdminInProgressUpdatedToday(req,this.currentBranchId).subscribe({
      next: (res) => {
  const data = res.data;

  this.rows = (data?.data ?? []).map((x: any) => ({
    ...x,
    id: x.taskId,     // ✅ مهم جداً عشان GenericTable
  }));

  this.totalItems = data?.totalCount ?? 0;
  this.page = data?.pageIndex ?? this.page;
  this.entries = data?.pageSize ?? this.entries;
  this.isLoading = false;
},

      error: () => {
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.page = page;
    this.searchCriteria.pageIndex = page;
    this.loadData();
  }

  onEntriesChange(entries: number) {
    this.entries = entries;
    this.searchCriteria.pageSize = entries;
    this.page = 1;
    this.searchCriteria.pageIndex = 1;
    this.loadData();
  }

  applyFilters(filters: any) {
    this.searchCriteria = {
      ...this.searchCriteria,
      ...filters,
      pageIndex: 1
    };
    this.page = 1;
    this.loadData();
  }

  checkRowClickable(item: any): boolean { return true; }

  onEdit(id: number) {

    console.log('Editing task ID:', id);
  const task = this.rows.find(x => x.taskId === id);
  console.log('Task object:', task);
  
    const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      windowClass: 'task-details-modal',
      backdrop: 'static',
      scrollable: true
    });

    modalRef.componentInstance.taskId = id;
    modalRef.componentInstance.readonly = true;
  }
}
