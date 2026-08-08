import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { TranslateModule } from "@ngx-translate/core";
import { GenericTableComponent, TableColumn } from "app/shared/components/generic-table/generic-table.component";
import { PenalityDto } from "app/core/models/dashboard/dashboard.model";

@Component({
  standalone: true,
  imports: [CommonModule, TranslateModule, GenericTableComponent],
  template: `
    <div class="modal-header">
      <h5 class="modal-title">{{ 'DASHBOARD.PENALTIES' | translate }}</h5>
      <button type="button" class="btn-close" (click)="activeModal.close()"></button>
    </div>

    <div class="modal-body p-0">
      <app-generic-table
        [columns]="columns"
        [data]="rows"
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
export class PenalitiesPopupComponent {
  @Input() Penalities: PenalityDto[] = [];

  columns: TableColumn[] = [
    { key: 'task', label: 'TASK.TASK_TITLE' },
    { key: 'createdDate', label: 'TASK.DATE', type: 'date' },
    { key: 'reason', label: 'TASK.PENALTY_REASON' },
    { key: 'amount', label: 'TASK.PENALTY_AMOUNT_LABEL' ,type: 'custom' },

  ];

  rows: any[] = [];

  constructor(public activeModal: NgbActiveModal) {}

  ngOnInit() {
    this.rows = this.Penalities.map(w => ({
      task: w.taskTitle,  
      createdDate: w.createdDate,
      reason: w.reason,
      amount: w.amount
    }));
  }
}
