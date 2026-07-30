import { NgbActiveModal, NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { TranslateModule } from "@ngx-translate/core";
import { GenericTableComponent, TableColumn } from "app/shared/components/generic-table/generic-table.component";
import { TaskDetailsShellComponent } from "app/components/tasks/task-details/task-details-shell/task-details-shell.component";

export interface CompletedTaskDetail {
  taskId: number;
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
        [rowClickable]="true"
        [rowClickableCondition]="checkRowClickable"
        (edit)="onEdit($event)"
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

  constructor(public activeModal: NgbActiveModal, private modalService: NgbModal) {}

 ngOnInit() {
  this.rows = this.tasks.map(t => ({
    id: t.taskId,
    taskTitle: t.taskTitle,       
    employeeNames: t.employeeNames, 
    createdAt: t.createdAt,
    closedAt: t.closedAt,
    duration: t.durationHours + 'h'
  }));
}
checkRowClickable(item: any): boolean {
    console.log('Row:', item);    
    return true;
  }

  onEdit(id: number) {
    const task = this.rows.find(x => x.taskId === id);

    const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      windowClass: 'task-details-modal',
      backdrop: 'static',
      scrollable: true
    });

    modalRef.componentInstance.taskId = id;
    modalRef.componentInstance.readonly = true;
    modalRef.componentInstance.createdByMe = task?.createdByMe ?? false;
  }
}
