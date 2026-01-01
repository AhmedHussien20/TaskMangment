import { Component, Input, OnInit, OnChanges, SimpleChanges, AfterViewInit, ViewChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { forkJoin } from 'rxjs';
import { TaskExtensionRequestService } from 'app/core/services/task-extension-request.service';
import { TaskCloseRequestService } from 'app/core/services/task-close-request.service';
import { BaseResponse } from 'app/models/base.response.model';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ExtensionRequestStatus, TaskExtensionRequestGet, TaskExtensionReviewDto } from 'app/core/models/task/task-extension-request';
import { FormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ToastrService } from 'ngx-toastr';
import { CloseRequestStatus, TaskCloseRequestGet } from 'app/core/models/task/task-close-request';
import { TaskService } from 'app/core/services/task.service';


@Component({
  selector: 'app-task-requests',
  standalone: true,
  imports: [CommonModule, GenericTableComponent, TranslateModule, FormsModule],
  templateUrl: './task-requests.component.html'
})
export class TaskRequestsComponent implements OnInit, OnChanges, AfterViewInit {

  @Input() taskId!: number;

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
    { key: 'response', label: 'TASK.REQUEST_REPLY' },
    { key: 'responseDate',label: 'TASK.REQUEST_REPLY_DATE' , type: 'date'},
    { key: 'review', label: 'TABLE.ACTIONS', type: 'icon-action', icon: 'bi bi-pencil-square' },

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

  ngOnInit(): void {
    console.log('TaskRequestsComponent ngOnInit, taskId:', this.taskId);
  }

  ngOnChanges(changes: SimpleChanges): void {
    console.log('TaskRequestsComponent ngOnChanges, taskId:', this.taskId);

    if (changes['taskId'] && this.taskId && this.initialized) {
      console.log('Task ID changed, reloading...');
      this.page = 1;
      this.loadRequests();
    }
  }

  ngAfterViewInit(): void {
    console.log('TaskRequestsComponent ngAfterViewInit');

    setTimeout(() => {
      this.initialized = true;
      if (this.taskId) {
        console.log('Loading requests after view init');
        this.loadRequests();
      }
    }, 0);
  }

 loadRequests(): void {
  if (!this.taskId) {
    console.error('Cannot load requests: taskId is undefined');
    return;
  }

  console.log('Loading requests for taskId:', this.taskId);
  this.isLoading = true;

  this.taskService.getTaskRequests(this.taskId).subscribe({
    next: (res) => {
      if (!res.success || !res.data) {
        this.rows = [];
        this.totalItems = 0;
        this.isLoading = false;
        return;
      }

      // Extension Requests
     // Extension Requests
// Extension Requests
const extensionRows = res.data.extensionRequests.map((x: TaskExtensionRequestGet) => ({
  requestNo: x.id,
  type: 'extend',
  sender: x.requestedByName || '',
  createdAt: x.requestedAt,
  comment: x.reason || '',
  response: this.translate.instant(this.getStatusText(x.extendRequestText)), 
  responseDate: x.reviewedAt || null
}));

const closeRows = res.data.closeRequests.map((x: TaskCloseRequestGet) => ({
  requestNo: x.id,
  type: 'close',
  sender: x.requestedByName || '',
  createdAt: x.requestedAt,
  comment: x.message || '',
  response: this.translate.instant(this.getStatusText(x.closeRequestText)), // استخدم closeRequestText
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
  this.modalService.open(modal, { size: 'lg', centered: true });
}



  submitReview() {
    if (!this.selectedRequestId || !this.reviewModel.status) return;
    if (this.reviewModel.status === ExtensionRequestStatus.Approved && !this.reviewModel.newDueDate) {
        this.toastr.warning(this.translate.instant('TASK.ENTER_NEW_DATE'));
      return;}
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