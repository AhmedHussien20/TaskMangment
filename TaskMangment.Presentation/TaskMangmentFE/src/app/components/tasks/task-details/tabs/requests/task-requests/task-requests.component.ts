import { Component, Input, OnInit, OnChanges, SimpleChanges, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';
import { forkJoin } from 'rxjs';
import { TaskExtensionRequestService } from 'app/core/services/task-extension-request.service';
import { TaskCloseRequestService } from 'app/core/services/task-close-request.service';
import { BaseResponse } from 'app/models/base.response.model';

@Component({
  selector: 'app-task-requests',
  standalone: true,
  imports: [CommonModule, GenericTableComponent],
  templateUrl: './task-requests.component.html'
})
export class TaskRequestsComponent implements OnInit, OnChanges, AfterViewInit {

  @Input() taskId!: number;

  isLoading = false;
  rows: any[] = [];
  totalItems = 0;
  page = 1;
  entries = 10;

  columns = [
    { key: 'requestNo', label: 'رقم الطلب' },
    {
      key: 'type',
      label: 'نوع الطلب',
      badgeMap: {
        close: { text: 'إغلاق', class: 'bg-danger' },
        extend: { text: 'تمديد', class: 'bg-warning' }
      }
    },
    { key: 'sender', label: 'المرسل' },
    { key: 'createdAt', label: 'التاريخ' },
    { key: 'comment', label: 'التعليق' },
    { key: 'response', label: 'الرد' },
    { key: 'responseDate', label: 'تاريخ الرد' }
  ];

  private initialized = false;

  constructor(
    private extensionService: TaskExtensionRequestService,
    private closeService: TaskCloseRequestService
  ) {
    console.log('TaskRequestsComponent CREATED');
  }

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

    // استخدام type assertion هنا
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
          requestNo: x.id,
          type: 'extend',
          sender: x.requestedByName,
          createdAt: x.requestedAt,
          comment: x.reason,
          response: this.getStatusText(x.status),
          responseDate: x.reviewedAt || null
        }));

        const closeRows = closeData.map((x: any) => ({
          requestNo: x.id,
          type: 'close',
          sender: x.requestedByName,
          createdAt: x.requestedAt,
          comment: x.message || x.reason || '',
          response: this.getStatusText(x.status),
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

  // دالة مساعدة لتحويل status إلى نص
  private getStatusText(status: number): string {
    if (status === undefined || status === null) return 'غير معروف';
    
    switch(status) {
      case 1: return 'قيد الانتظار';
      case 2: return 'مقبول';
      case 3: return 'مرفوض';
      default: return `حالة ${status}`;
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
}