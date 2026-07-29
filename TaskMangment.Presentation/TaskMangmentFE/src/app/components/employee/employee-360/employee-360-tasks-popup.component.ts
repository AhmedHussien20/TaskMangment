import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbActiveModal, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { TaskDetailsShellComponent } from 'app/components/tasks/task-details/task-details-shell/task-details-shell.component';
import { EmployeeService } from 'app/core/services/employee.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';

export type Employee360TaskFilter = 'active' | 'overdue' | 'completed' | 'week' | 'linked';

@Component({
  standalone: true,
  selector: 'app-employee-360-tasks-popup',
  imports: [CommonModule, TranslateModule, GenericTableComponent],
  template: `
    <div class="modal-header">
      <h5 class="modal-title">{{ titleKey | translate }}</h5>
      <button type="button" class="btn-close" (click)="activeModal.close()"></button>
    </div>

    <div class="modal-body p-0">
      <div *ngIf="loading" class="p-4 text-center text-muted">{{ 'COMMON.LOADING' | translate }}</div>
      <app-generic-table
        *ngIf="!loading"
        [columns]="columns"
        [data]="rows"
        [page]="page"
        [entries]="pageSize"
        [totalItems]="totalItems"
        [showAddButton]="false"
        [showEditButton]="false"
        [showDeleteButton]="false"
        [showDetailsButton]="false"
        [showFilters]="false"
        [showPagination]="filter !== 'linked'"
        [searchCriteria]="emptyCriteria"
        [rowClickable]="true"
        [rowClickableCondition]="rowClickable"
        (edit)="openDetails($event)"
        (pageChange)="onPageChange($event)"
        (entriesChange)="onEntriesChange($event)">
      </app-generic-table>
    </div>

    <div class="modal-footer">
      <button type="button" class="btn btn-outline-secondary btn-sm" (click)="activeModal.close()">
        {{ 'COMMON.CANCEL' | translate }}
      </button>
    </div>
  `
})
export class Employee360TasksPopupComponent implements OnInit {
  @Input() titleKey = 'TASK.LIST_TITLE';
  @Input() employeeId!: number;
  @Input() filter: Employee360TaskFilter = 'active';
  @Input() from = '';
  @Input() to = '';
  /** Preloaded rows for linked-discount tasks (already enriched from discounts API). */
  @Input() linkedTasks: Array<{
    id: number;
    title?: string;
    statusText?: string;
    assignedByName?: string;
    dueDate?: string | null;
    createdByMe?: boolean;
  }> = [];

  loading = false;
  rows: any[] = [];
  totalItems = 0;
  page = 1;
  pageSize = 10;

  emptyCriteria: SearchCriteria = {
    searchKey: '',
    pageIndex: 1,
    pageSize: 10,
    sortColumn: 'Id',
    sortDirection: 'DESC'
  };

  columns: TableColumn[] = [
    { key: 'task', label: 'TASK.TITLE' },
    {
      key: 'statusText',
      label: 'TASK.STATUS',
      type: 'badge',
      badgeMap: {
        New: { text: 'TASK.STATUS_NEW', class: 'bg-secondary' },
        InProgress: { text: 'TASK.STATUS_IN_PROGRESS', class: 'bg-info' },
        Closed: { text: 'TASK.STATUS_CLOSED', class: 'bg-success' },
        Archived: { text: 'TASK.STATUS_ARCHIVED', class: 'bg-dark' },
        AutoClose: { text: 'TASK.STATUS_AUTOCLOSE', class: 'bg-warning' }
      }
    },
    { key: 'assignedByName', label: 'TASK.ASSIGNED_BY' },
    { key: 'dueDate', label: 'TASK.DUE_DATE', type: 'date' }
  ];

  rowClickable = (): boolean => true;

  constructor(
    public activeModal: NgbActiveModal,
    private modalService: NgbModal,
    private employeeService: EmployeeService
  ) {}

  ngOnInit(): void {
    if (this.filter === 'linked') {
      this.rows = (this.linkedTasks || []).map(t => ({
        id: t.id,
        task: `[${t.id}] ${t.title || ''}`,
        statusText: t.statusText || '',
        assignedByName: t.assignedByName || '',
        dueDate: t.dueDate ?? null,
        createdByMe: t.createdByMe ?? false
      }));
      this.totalItems = this.rows.length;
      this.pageSize = Math.max(this.rows.length, 1);
      return;
    }
    this.loadPage();
  }

  loadPage(): void {
    this.loading = true;
    this.employeeService
      .get360KpiTasks(
        this.employeeId,
        this.filter as 'active' | 'overdue' | 'week' | 'completed',
        this.page,
        this.pageSize
      )
      .subscribe({
        next: (res: any) => {
          const page = res?.data;
          const list = page?.data ?? [];
          this.rows = list.map((t: any) => ({
            id: t.id,
            task: `[${t.id}] ${t.title}`,
            statusText: t.statusText,
            assignedByName: t.assignedByName,
            dueDate: t.dueDate,
            createdByMe: t.createdByMe ?? false
          }));
          this.totalItems = page?.totalCount ?? this.rows.length;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.rows = [];
          this.totalItems = 0;
        }
      });
  }

  onPageChange(page: number): void {
    this.page = page;
    this.loadPage();
  }

  onEntriesChange(entries: number): void {
    this.pageSize = entries;
    this.page = 1;
    this.loadPage();
  }

  openDetails(id: number): void {
    const row = this.rows.find(r => r.id === id);
    const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      backdrop: 'static',
      scrollable: true
    });
    modalRef.componentInstance.taskId = id;
    modalRef.componentInstance.readonly = true;
    modalRef.componentInstance.createdByMe = row?.createdByMe ?? false;
  }
}
