import { Component, Input, OnInit, OnChanges, SimpleChanges, AfterViewInit, ViewChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { forkJoin } from 'rxjs';
import { TaskExtensionRequestService } from 'app/core/services/task-extension-request.service';
import { TaskCloseRequestService } from 'app/core/services/task-close-request.service';
import { BaseResponse } from 'app/models/base.response.model';
import { TranslateModule, TranslateService} from '@ngx-translate/core';
import { ExtensionRequestStatus, TaskExtensionRequestGet, TaskExtensionReviewDto } from 'app/core/models/task/task-extension-request';
import { FormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ToastrService } from 'ngx-toastr';
import { CloseRequestStatus, TaskCloseRequestGet } from 'app/core/models/task/task-close-request';
import { TaskService } from 'app/core/services/task.service';
import { NgSelectModule } from '@ng-select/ng-select';
import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';
import { isPastDueDate, isWeekendDueDate } from 'app/shared/validations/weekend-due-date.validator';

@Component({
  selector: 'app-task-requests',
  standalone: true,
  imports: [CommonModule, GenericTableComponent, TranslateModule, FormsModule,NgSelectModule,DatePickerComponent],
  templateUrl: './task-requests.component.html'
})
export class TaskRequestsComponent implements OnInit, OnChanges, AfterViewInit {

  @Input() taskId!: number;
  /** Already gated by shell: createdByMe && approve/reject/extend permission. */
  @Input() createdByMe: boolean = false;
  canReview: boolean = false;

  isLoading = false;
  rows: any[] = [];
  totalItems = 0;
  page = 1;
  entries = 10;

  reviewModel: TaskExtensionReviewDto = { status: ExtensionRequestStatus.Pending };
  requestType: 'extend' | 'close' = 'extend';
  ExtensionRequestStatus = ExtensionRequestStatus;
  selectedRequestId: number | null = null;
  showReviewModal = false;
  CloseRequestStatus = CloseRequestStatus;
  selectedCloseStatus: CloseRequestStatus | null = null;


  columns: TableColumn[] = [
    { key: 'requestNo', label: 'TASK.REQUEST_NUMBER' },
    {
      key: 'type',
      label: 'TASK.REQUEST_TYPE',
      type: 'badge',
      badgeMap: {
        close: {
          text: 'TASK.REQUEST_TYPE_CLOSE',
          class: 'bg-danger',
          icon: 'bi bi-x-circle'
        },
        extend: {
          text: 'TASK.REQUEST_TYPE_EXTEND',
          class: 'bg-warning',
          icon: 'bi bi-arrow-repeat'
        }
      }
    },
    { key: 'sender', label: 'TASK.REQUEST_SENDER' },
    { key: 'createdAt', label: 'TASK.DATE', type: 'date' },
    { key: 'comment', label: 'TASK.REQUEST_COMMENT' },
{
  key: 'response',
  label: 'TASK.REQUEST_REPLY',
  type: 'badge',
  badgeMap: {
    Pending: { text: 'TASK.REQUEST_STATUS_PENDING', class: 'bg-warning' },
    Approved: { text: 'TASK.REQUEST_STATUS_APPROVED', class: 'bg-success' },
    Rejected: { text: 'TASK.REQUEST_STATUS_REJECTED', class: 'bg-danger' },
    Unknown: { text: 'TASK.REQUEST_STATUS_UNKNOWN', class: 'bg-dark' },
  }
},    { key: 'responseDate',label: 'TASK.REQUEST_REPLY_DATE' , type: 'date'},

  ];

  private initialized = false;

 
  constructor(
    private extensionService: TaskExtensionRequestService,
    private closeService: TaskCloseRequestService,
    private translate: TranslateService,
    private modalService: NgbModal,
    private toastr: ToastrService,
    private taskService: TaskService
  ) { }

 extensionStatusOptions: { label: string; value: ExtensionRequestStatus }[] = [];
closeStatusOptions: { label: string; value: CloseRequestStatus }[] = [];

ngOnInit(): void {
  this.refreshCanReview();
  this.extensionStatusOptions = [
    { label: this.translate.instant('TASK.STATUS_APPROVED'), value: ExtensionRequestStatus.Approved },
    { label: this.translate.instant('TASK.STATUS_REJECTED'), value: ExtensionRequestStatus.Rejected },
  ];

  this.closeStatusOptions = [
    { label: this.translate.instant('TASK.STATUS_APPROVED'), value: CloseRequestStatus.Approved },
    { label: this.translate.instant('TASK.STATUS_REJECTED'), value: CloseRequestStatus.Rejected },
  ];

  this.ensureReviewColumn();
}
  ngOnChanges(changes: SimpleChanges): void {
    if (changes['createdByMe']) {
      this.refreshCanReview();
      this.ensureReviewColumn();
    }
    if (changes['taskId'] && this.taskId && this.initialized) {
      this.page = 1;
      this.loadRequests();
    }
  }

  private refreshCanReview(): void {
    // Shell passes createdByMe && canReviewRequests — do not re-OR permissions here.
    this.canReview = this.createdByMe;
  }

  private ensureReviewColumn(): void {
    const hasReviewCol = this.columns.some(c => c.key === 'review');
    if (this.canReview && !hasReviewCol) {
      this.columns.push({
        key: 'review',
        label: 'TABLE.ACTIONS',
        type: 'icon-action',
        icon: 'bi bi-pencil-square'
      });
    }
    if (!this.canReview && hasReviewCol) {
      this.columns = this.columns.filter(c => c.key !== 'review');
    }
  }

  ngAfterViewInit(): void {

    setTimeout(() => {
      this.initialized = true;
      if (this.taskId) {
        this.loadRequests();
      }
    }, 0);
  }

 loadRequests(): void {
  if (!this.taskId) {
    console.error('Cannot load requests: taskId is undefined');
    return;
  }

  this.isLoading = true;

  this.taskService.getTaskRequests(this.taskId).subscribe({
    next: (res) => {
      if (!res.success || !res.data) {
        this.rows = [];
        this.totalItems = 0;
        this.isLoading = false;
        return;
      }


const extensionRows = res.data.extensionRequests.map((x: TaskExtensionRequestGet) => ({
  requestNo: x.id,
  type: 'extend',
  sender: x.requestedByName || '',
  createdAt: x.requestedAt,
  comment: x.reason || '',
  response: x.extendRequestText || 'Unknown', 
  responseDate: x.reviewedAt || null
}));

const closeRows = res.data.closeRequests.map((x: TaskCloseRequestGet) => ({
  requestNo: x.id,
  type: 'close',
  sender: x.requestedByName || '',
  createdAt: x.requestedAt,
  comment: x.message || '',
  response: x.closeRequestText || 'Unknown', 
  responseDate: x.reviewedAt || null
}));

      this.rows = [...extensionRows, ...closeRows]
        .filter(item => item.createdAt)
        .sort((a, b) => new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime());

      this.totalItems = this.rows.length;
      this.isLoading = false;

      console.log('Rows loaded:', this.rows.length, 'items');
    },
    error: (err) => {
      console.error('Error loading requests:', err);
      this.isLoading = false;
    }
  });
}



  private getStatusText(status: string): string {
  switch (status) {
    case 'Pending':
      return 'TASK.REQUEST_STATUS_PENDING';
    case 'Approved':
      return 'TASK.REQUEST_STATUS_APPROVED';
    case 'Rejected':
      return 'TASK.REQUEST_STATUS_REJECTED';
    default:
      return 'TASK.REQUEST_STATUS_UNKNOWN';
  }
}




  refresh(): void {
    console.log('Refreshing requests');
    this.loadRequests();
  }

  onPageChange(page: number): void {
    console.log('Page changed to:', page);
    this.page = page;
    this.loadRequests();
  }



 openReviewModal(
  requestNo: number,
  modal: any,
  requestType: 'extend' | 'close',
  reviewedAt?: string
) {
  if (reviewedAt) {
    this.toastr.warning(this.translate.instant('TASK.ALREADY_REVIEWED'));
    return;
  }

  this.selectedRequestId = requestNo;
  this.requestType = requestType;
  this.reviewModel = { status: null };
  this.selectedCloseStatus = null;
  this.modalService.open(modal, { size: 'm', centered: true });
}



  submitReview() {
    if (!this.selectedRequestId || !this.reviewModel.status) return;
    if (this.reviewModel.status === ExtensionRequestStatus.Approved && !this.reviewModel.newDueDate) {
        this.toastr.warning(this.translate.instant('TASK.ENTER_NEW_DATE'));
      return;}
    if (this.reviewModel.status === ExtensionRequestStatus.Approved && isWeekendDueDate(this.reviewModel.newDueDate)) {
      this.toastr.error(this.translate.instant('TASK.DUE_DATE_WEEKEND'));
      return;
    }
    if (this.reviewModel.status === ExtensionRequestStatus.Approved && isPastDueDate(this.reviewModel.newDueDate)) {
      this.toastr.error(this.translate.instant('TASK.DUE_DATE_PAST'));
      return;
    }
    this.extensionService.review(this.selectedRequestId, this.reviewModel).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('TASK.SAVED_SUCCESS'));
        this.modalService.dismissAll();
        this.refresh();
      }
    });
  }
submitCloseReview(status: CloseRequestStatus | null) {
  if (!this.selectedRequestId || !status) return;
  this.closeService.review(this.selectedRequestId, status)
    .subscribe(() => {
      this.toastr.success(this.translate.instant('TASK.SAVED_SUCCESS'));
      this.modalService.dismissAll();
      this.refresh();
    });
}


 onAction(event: { type: string; row: any }, modal: any) {
  const requestId = event.row.requestNo;
  const requestType = event.row.type;
  const reviewedAt = event.row.responseDate || null; 
  this.openReviewModal(requestId, modal, requestType, reviewedAt);
}


}