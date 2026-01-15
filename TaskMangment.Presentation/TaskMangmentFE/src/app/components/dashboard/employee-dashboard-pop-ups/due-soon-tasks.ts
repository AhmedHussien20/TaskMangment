import { Component, Input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { MyTaskDto } from 'app/core/models/dashboard/dashboard.model';

@Component({
  standalone: true,
  selector: 'app-employee-tasks-popup',
  imports: [
    CommonModule,
    TranslateModule,
    GenericTableComponent
  ],
  template: `
    <div class="modal-header">
      <h5 class="modal-title">{{ 'DASHBOARD.DUE_SOON' | translate }}</h5>
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
        [showPagination]="false">
      </app-generic-table>
    </div>

    <div class="modal-footer">
      <button class="btn btn-outline-secondary btn-sm" (click)="activeModal.close()">
        {{ 'COMMON.CANCEL' | translate }}
      </button>
    </div>
  `
})
export class MyDueSoonTasksPopupComponent {
  @Input() tasks: MyTaskDto[] = [];

  columns: TableColumn[] = [
    { key: 'task', label: 'TASK.TASK_TITLE' },
    {
      key: 'statusText',
      label: 'TASK.STATUS',
      type: 'badge',
      badgeMap: {
        1: { text: 'TASK.STATUS_NEW', class: 'bg-secondary' },
        2: { text: 'TASK.STATUS_IN_PROGRESS', class: 'bg-info' },
        3: { text: 'TASK.STATUS_CLOSED', class: 'bg-success' },
        4: { text: 'TASK.STATUS_ARCHIVED', class: 'bg-dark' },
        5: { text: 'TASK.STATUS_AUTOCLOSE', class: 'bg-warning' }
      }
    },
    { key: 'dueDate', label: 'TASK.DUE_DATE', type: 'date' },
    { key: 'progressPercent', label: 'TASK.ACHIEVEMENT' }
  ];

  rows: any[] = [];

  constructor(public activeModal: NgbActiveModal) {}

  ngOnInit() {
    this.mapRows();
  }

  private mapRows() {
    this.rows = this.tasks.map(t => ({
      task: `[${t.taskId}] ${t.title}`,
      statusText: t.status,
      dueDate: t.dueDate,
      progressPercent: t.progressPercent
    }));
  }
}
