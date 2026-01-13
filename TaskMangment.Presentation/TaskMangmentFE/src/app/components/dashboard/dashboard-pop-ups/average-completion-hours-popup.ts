import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { TranslateModule } from "@ngx-translate/core";
import { GenericTableComponent, TableColumn } from "app/shared/components/generic-table/generic-table.component";

export interface CompletedTaskDetail {
  taskTitle: string;
  employeeNames: string;
  createdAt: string;
  closedAt: string;
  durationHours: number;
}

@Component({
  standalone: true,
  imports: [CommonModule, TranslateModule, GenericTableComponent],
  template: `
    <div class="modal-header">
      <h5 class="modal-title">{{ 'DASHBOARD.AVG_COMPLETION_TIME' | translate }}</h5>
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
export class CompletedTasksPopupComponent {
  @Input() tasks: CompletedTaskDetail[] = [];

  columns: TableColumn[] = [
    { key: 'taskTitle', label: 'TASK.TASK_TITLE' },
    { key: 'employeeNames', label: 'TASK.EMPLOYEES' },
    { key: 'createdAt', label: 'TASK.CREATED_AT', type: 'date' },
    { key: 'closedAt', label: 'TASK.CLOSED_AT', type: 'date' },
    { key: 'duration', label: 'TASK.DURATION' }
  ];

  rows: any[] = [];

  constructor(public activeModal: NgbActiveModal) {}

 ngOnInit() {
  this.rows = this.tasks.map(t => ({
    taskTitle: t.taskTitle,       
    employeeNames: t.employeeNames, 
    createdAt: t.createdAt,
    closedAt: t.closedAt,
    duration: t.durationHours + 'h'
  }));
}

}
