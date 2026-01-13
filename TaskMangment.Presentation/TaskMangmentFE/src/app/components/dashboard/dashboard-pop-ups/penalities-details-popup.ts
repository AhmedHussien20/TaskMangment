import { NgbActiveModal } from "@ng-bootstrap/ng-bootstrap";
import { Component, Input } from "@angular/core";
import { CommonModule } from "@angular/common";
import { TranslateModule } from "@ngx-translate/core";
import { GenericTableComponent, TableColumn } from "app/shared/components/generic-table/generic-table.component";
import { DiscountGetDto } from "app/core/models/task/task-penalty";



@Component({
  standalone: true,
  imports: [CommonModule, TranslateModule, GenericTableComponent],
  template: `
    <div class="modal-header">
      <h5 class="modal-title">{{ 'TASK.PENALTIES' | translate }}</h5>
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
export class DiscountsPopupComponent {
  @Input() discounts: DiscountGetDto[] = [];

  columns: TableColumn[] = [
    { key: 'employeeName', label: 'TASK.PENALTY_EMPLOYEE_NAME' },
    { key: 'task', label: 'TASK.TASK_TITLE' },
    { key: 'createdDate', label: 'TASK.DATE', type: 'date' },
    { key: 'reason', label: 'TASK.PENALTY_REASON' },
    { key: 'amount', label: 'TASK.PENALTY_AMOUNT' }
  ];

  rows: any[] = [];

  constructor(public activeModal: NgbActiveModal) {}

  ngOnInit() {
  this.rows = this.discounts.map(d => ({
    employeeName: d.employeeName, 
    task: d.taskTitle,            
    createdDate: d.createdDate,
    reason: d.reason,
    amount: d.amount + ' SAR'
  }));
}

}
