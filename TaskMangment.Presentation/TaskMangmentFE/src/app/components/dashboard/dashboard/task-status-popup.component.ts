import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { Component } from '@angular/core';
import { TaskStatusDto } from "app/core/models/dashboard/dashboard.model";
import { MyDatePipe } from "app/components/utilities/pipline/MyDatePipe";
import { CommonModule } from "@angular/common";
import { TaskStatus } from "app/core/models/task/task";
import { TranslateModule } from "@ngx-translate/core";
import { GenericTableComponent, TableColumn } from "app/shared/components/generic-table/generic-table.component";

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
      <button class="btn btn-outline-secondary btn-sm" (click)="activeModal.close()">
        {{ 'COMMON.CANCEL' | translate }}
      </button>
    </div>
  `
})
export class TaskStatusPopupComponent {
  tasks: TaskStatusDto[] = [];

  /** Columns */
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
}
,

  { key: 'dueDate', label: 'TASK.DUE_DATE', type: 'date' },
  { key: 'employees', label: 'TASK.EMPLOYEES' }
];


  /** Rows for generic table */
  rows: any[] = [];

  constructor(public activeModal: NgbActiveModal) {}

  ngOnInit() {
    this.mapRows();
  }

 private mapRows() {
  this.rows = this.tasks.map(t => ({
    task: `[${t.taskId}] ${t.title}`,
    statusText: t.statusText,   
    dueDate: t.dueDate,
    employees: t.employees.join('، ')
  }));
}

}
