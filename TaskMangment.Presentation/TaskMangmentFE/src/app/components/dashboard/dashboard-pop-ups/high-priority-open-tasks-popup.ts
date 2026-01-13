import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import {
  GenericTableComponent,
  TableColumn,
} from 'app/shared/components/generic-table/generic-table.component';

export interface HighPriorityTask {
  taskTitle: string;
  employees: string[];
  statusText: string;
  dueDate: string;
}

@Component({
  standalone: true,
  imports: [CommonModule, TranslateModule, GenericTableComponent],
  template: `
    <div class="modal-header">
      <h5 class="modal-title">{{ 'DASHBOARD.HIGH_PRIORITY' | translate }}</h5>
      <button
        type="button"
        class="btn-close"
        (click)="activeModal.close()"
      ></button>
    </div>

    <div class="modal-body p-0">
      <app-generic-table
        [columns]="columns"
        [data]="rows"
        [page]="1"
        [entries]="rows.length"
        [totalItems]="rows.length"
        [showAddButton]="false"
        [showEditButton]="false"
        [showDeleteButton]="false"
        [showFilters]="false"
        [showPagination]="false"
      >
      </app-generic-table>
    </div>

    <div class="modal-footer">
      <button
        class="btn btn-outline-secondary btn-sm"
        (click)="activeModal.close()"
      >
        {{ 'COMMON.CANCEL' | translate }}
      </button>
    </div>
  `,
})
export class HighPriorityTasksPopupComponent {
  @Input() tasks: HighPriorityTask[] = [];

  columns: TableColumn[] = [
    { key: 'title', label: 'TASK.TASK_TITLE' },
    { key: 'employees', label: 'TASK.EMPLOYEES' },
    {
      key: 'statusText',
      label: 'TASK.STATUS',
      type: 'badge',
      badgeMap: {
        New: { text: 'TASK.STATUS_NEW', class: 'bg-secondary' },
        InProgress: { text: 'TASK.STATUS_IN_PROGRESS', class: 'bg-info' },
        Closed: { text: 'TASK.STATUS_CLOSED', class: 'bg-success' },
        Archived: { text: 'TASK.STATUS_ARCHIVED', class: 'bg-dark' },
        AutoClose: { text: 'TASK.STATUS_AUTOCLOSE', class: 'bg-warning' },
      },
    },
    { key: 'dueDate', label: 'TASK.DUE_DATE', type: 'date' },
  ];

  rows: any[] = [];

  constructor(public activeModal: NgbActiveModal) {}

  ngOnInit() {
    this.rows = this.tasks.map((t) => ({
      title: t.taskTitle, // مطابق للـ column key
      employees: t.employees.join('، '),
      statusText: t.statusText,
      dueDate: t.dueDate,
    }));
  }
}
