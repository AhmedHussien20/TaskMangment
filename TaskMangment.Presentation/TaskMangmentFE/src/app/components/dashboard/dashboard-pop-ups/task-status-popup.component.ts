import { Component, Input } from '@angular/core';
import { NgbActiveModal, NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { CommonModule } from "@angular/common";
import { TranslateModule } from "@ngx-translate/core";
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { TaskDetailsShellComponent } from 'app/components/tasks/task-details/task-details-shell/task-details-shell.component';
import { TaskStatusDto } from 'app/core/models/dashboard/dashboard.model';
import { DashboardService } from 'app/core/services/dashboar.service';

@Component({
  standalone: true,
  imports: [
    CommonModule,
    TranslateModule,
    GenericTableComponent
  ],
  template: `
    <div class="modal-header">
      <h5 class="modal-title">{{ 'TASK.LIST_TITLE' | translate }}</h5>
      <button type="button" class="btn-close" (click)="activeModal.close()"></button>
    </div>

    <div class="modal-body p-0">
      <app-generic-table
        [columns]="columns"
        [data]="tasksRows"
        [page]="request.pageIndex"
        [entries]="request.pageSize"
        [totalItems]="totalItems"
        [showAddButton]="false"
        [showEditButton]="false"
        [showDeleteButton]="false"
        [showFilters]="false"
        [showPagination]="true"
        [rowClickable]="true"
        [rowClickableCondition]="checkRowClickable"
        (edit)="onEdit($event)"
        (pageChange)="onPageChange($event)"
        (entriesChange)="onEntriesChange($event)"
      >
      </app-generic-table>
    </div>

    <div class="modal-footer">
      <button class="btn btn-outline-secondary btn-sm" (click)="activeModal.close()">
        {{ 'COMMON.CANCEL' | translate }}
      </button>
    </div>
  `
})
export class TaskStatusPopupComponent {

  @Input() status: 'Active' | 'Overdue' | 'Completed' | 'New' = 'Active';
  @Input() request: any = { pageIndex: 1, pageSize: 20, period: null };
  @Input() branchId?: number;

  tasks: TaskStatusDto[] = [];
  tasksRows: any[] = [];
  totalItems = 0;

  columns: TableColumn[] = [
    { key: 'task', label: 'TASK.TASK_TITLE' },
    {
      key: 'statusText',
      label: 'TASK.STATUS',
      type: 'badge',
      badgeMap: {
        New:        { text: 'TASK.STATUS_NEW',        class: 'bg-secondary' },
        InProgress: { text: 'TASK.STATUS_IN_PROGRESS', class: 'bg-info' },
        Closed:     { text: 'TASK.STATUS_CLOSED',     class: 'bg-success' },
        Archived:   { text: 'TASK.STATUS_ARCHIVED',   class: 'bg-dark' },
        AutoClose:  { text: 'TASK.STATUS_AUTOCLOSE',  class: 'bg-warning' }
      }
    },
    { key: 'employees', label: 'TASK.EMPLOYEES' },
    { key: 'createdDate', label: 'TASK.CREATED_AT', type: 'date' },
    { key: 'dueDate', label: 'TASK.DUE_DATE', type: 'date' }
  ];

  constructor(
    public activeModal: NgbActiveModal,
    private modalService: NgbModal,
    private dashboardService: DashboardService
  ) {}

  loadPageData() {
    if (!this.status || !this.request.period) return;

    this.dashboardService
      .getAdminTasksByStatus(this.status, this.request, this.branchId)
      .subscribe(res => {
        const data = res.data;
        this.tasks = data.data || [];
        this.totalItems = data.totalCount || 0;
        this.request.pageIndex = data.pageIndex || this.request.pageIndex;
        this.request.pageSize = data.pageSize || this.request.pageSize;

        this.mapRows();
      });
  }

  private mapRows() {
    this.tasksRows = this.tasks.map(t => ({
      id: t.taskId,
      taskId: t.taskId,
      task: `[${t.taskId}] ${t.title}`,
      statusText: t.statusText,
      createdDate: t.createdDate,
      dueDate: t.dueDate,
      employees: Array.isArray(t.employees) ? t.employees.join('، ') : '',
      createdByMe: t.createdByMe || false
    }));
  }

  onPageChange(page: number) {
    this.request.pageIndex = page;
    this.loadPageData();
  }

  onEntriesChange(entries: number) {
    this.request.pageSize = entries;
    this.request.pageIndex = 1;
    this.loadPageData();
  }

  checkRowClickable(item: any): boolean {
    return true;
  }

  onEdit(id: number) {
    const task = this.tasksRows.find(x => x.taskId === id);

    const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      backdrop: 'static',
      scrollable: true
    });

    modalRef.componentInstance.taskId = id;
    modalRef.componentInstance.readonly = true;
    modalRef.componentInstance.createdByMe = task?.createdByMe ?? false;
  }
}