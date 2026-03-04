import { Component, Input, OnInit } from "@angular/core";
import { NgbActiveModal, NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { CommonModule } from "@angular/common";
import { TranslateModule } from "@ngx-translate/core";
import { GenericTableComponent, TableColumn } from "app/shared/components/generic-table/generic-table.component";
import { TaskDetailsShellComponent } from "app/components/tasks/task-details/task-details-shell/task-details-shell.component";
import { DashboardService } from "app/core/services/dashboar.service";
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
export class DiscountsPopupComponent implements OnInit {
  @Input() request: any = { pageIndex: 1, pageSize: 20, period: null };
  @Input() branchId?: number;

  discounts: DiscountGetDto[] = [];
  rows: any[] = [];
  totalItems = 0;

  columns: TableColumn[] = [
    { key: 'employeeName', label: 'TASK.PENALTY_EMPLOYEE_NAME' },
    { key: 'task', label: 'TASK.TASK_TITLE' },
    { key: 'createdDate', label: 'TASK.DATE', type: 'date' },
    { key: 'reason', label: 'TASK.PENALTY_REASON' },
    { key: 'amount', label: 'TASK.PENALTY_AMOUNT' }
  ];

  constructor(
    public activeModal: NgbActiveModal,
    private modalService: NgbModal,
    private dashboardService: DashboardService
  ) {}

  ngOnInit() {
    this.loadPageData();
  }

  loadPageData() {
    this.dashboardService
      .getAdminDiscounts(this.request, this.branchId)
      .subscribe(res => {
        const data = res.data;
        this.discounts = data.data || [];
        this.totalItems = data.totalCount || 0;
        this.request.pageIndex = data.pageIndex || this.request.pageIndex;
        this.request.pageSize = data.pageSize || this.request.pageSize;

        this.mapRows();
      });
  }

  private mapRows() {
    this.rows = this.discounts.map(d => ({
      id: d.taskId,
      taskId: d.taskId,
      employeeName: d.employeeName,
      task: `[${d.taskId}] ${d.taskTitle}`,
      createdDate: d.createdDate,
      reason: d.reason,
      amount: d.amount + ' SAR',
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

  onEdit(taskId: number) {
    const task = this.rows.find(x => x.taskId === taskId);

    const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      backdrop: 'static',
      scrollable: true
    });

    modalRef.componentInstance.taskId = taskId;
    modalRef.componentInstance.readonly = true;
    modalRef.componentInstance.createdByMe = task?.createdByMe ?? false;
  }
}