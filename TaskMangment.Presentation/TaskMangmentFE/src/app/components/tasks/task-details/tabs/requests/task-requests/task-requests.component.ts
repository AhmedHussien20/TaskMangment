import { Component, Input, OnInit, OnChanges, SimpleChanges, AfterViewInit, ViewChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { forkJoin } from 'rxjs';
import { TaskExtensionRequestService } from 'app/core/services/task-extension-request.service';
import { TaskCloseRequestService } from 'app/core/services/task-close-request.service';
import { BaseResponse } from 'app/models/base.response.model';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ExtensionRequestStatus, TaskExtensionReviewDto } from 'app/core/models/task/task-extension-request';
import { FormsModule } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';


@Component({
  selector: 'app-task-requests',
  standalone: true,
  imports: [CommonModule, GenericTableComponent, TranslateModule,FormsModule],
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
ExtensionRequestStatus = ExtensionRequestStatus;
selectedRequestId: number | null = null;
showReviewModal = false;


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
  { key: 'responseDate', label: 'TASK.REQUEST_REPLY_DATE' },
  
];

  private initialized = false;

  constructor(
    private extensionService: TaskExtensionRequestService,
    private closeService: TaskCloseRequestService,
    private translate : TranslateService,
    private modalService: NgbModal,

  ) {}

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

    const requestPayload = {
      taskId: this.taskId,
      pageIndex: this.page,
      pageSize: this.entries,
      sortColumn: 'RequestedAt',
      sortDirection: 'desc',
      searchKey: ''
    };

    console.log('Request payload:', requestPayload);

    forkJoin({
      extensions: this.extensionService.getAll(requestPayload) as any,
      closes: this.closeService.getAll(requestPayload) as any
    }).subscribe({
      next: (res: any) => {
        
        const extensionResponse: BaseResponse<any> = res.extensions;
        const closeResponse: BaseResponse<any> = res.closes;
        
        const extensionData = extensionResponse?.success ? 
          (extensionResponse.data?.data || []) : [];
        
        const closeData = closeResponse?.success ? 
          (closeResponse.data?.data || []) : [];
        
      
        
        const extensionRows = extensionData.map((x: any) => ({
  requestNo: Number(x.id),
  type: 'extend',
  sender: typeof x.requestedByName === 'string' ? x.requestedByName : x.requestedByName?.name || '',
  createdAt: x.requestedAt,
  comment: x.reason || '',
  response: this.translate.instant(this.getStatusText(x.extendRequestText)),
  responseDate: x.reviewedAt || null
}));

const closeRows = closeData.map((x: any) => ({
  requestNo: Number(x.id),
  type: 'close',
  sender: typeof x.requestedByName === 'string' ? x.requestedByName : x.requestedByName?.name || '',
  createdAt: x.requestedAt,
  comment: x.message || x.reason || '',
  response: this.translate.instant(this.getStatusText(x.closeRequestText)),
  responseDate: x.reviewedAt || null
}));

        this.rows = [...extensionRows, ...closeRows]
          .filter(item => item.createdAt)
          .sort((a, b) => {
            const dateA = a.createdAt ? new Date(a.createdAt).getTime() : 0;
            const dateB = b.createdAt ? new Date(b.createdAt).getTime() : 0;
            
            return dateB - dateA;
          });

        this.totalItems = this.rows.length;
        this.isLoading = false;
        
        console.log('Rows loaded:', this.rows.length, 'items');
        if (this.rows.length > 0) {
          console.log('Sample rows:', this.rows);
        }
      },
      error: (error) => {
        console.error('Error loading requests:', error);
        this.isLoading = false;
      }
    });
  }

  private getStatusText(status: string): string {
  if (!status) return 'TASK.REQUEST_STATUS_UNKNOWN';

  switch (status) {
    case 'Pending':
      return 'TASK.REQUEST_STATUS_PENDING';
    case 'Approved':
      return 'TASK.REQUEST_STATUS_APPROVED';
    case 'Rejected':
      return 'TASK.REQUEST_STATUS_REJECTED';
    default:
      return status;
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

  

 openReviewModal(id: number, modal: any) {
  const row = this.rows.find(r => r.id === id);
  if (!row || row.type !== 'extend') return;

  this.selectedRequestId = row.id;   
  this.reviewModel = { status: ExtensionRequestStatus.Pending };
  this.modalService.open(modal, { size: 'lg', centered: true });
}



submitReview() {
console.log('submitReview clicked', this.selectedRequestId, this.reviewModel);
  if (!this.selectedRequestId || !this.reviewModel.status) return;

  if (this.reviewModel.status === ExtensionRequestStatus.Approved && !this.reviewModel.newDueDate) {
    alert('Please select a new due date');
    return;
  }

  this.extensionService.review(this.selectedRequestId, this.reviewModel).subscribe({
    next: () => {
      this.modalService.dismissAll();
      this.refresh(); 
    },
    error: (err) => {
      console.error(err);
      alert('Failed to review request');
    }
  });
}
}