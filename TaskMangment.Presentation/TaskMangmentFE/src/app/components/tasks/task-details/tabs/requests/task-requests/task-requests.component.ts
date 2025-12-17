import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';

@Component({
  selector: 'app-task-requests',
  standalone: true,
  imports: [CommonModule, GenericTableComponent],
  templateUrl: './task-requests.component.html'
})
export class TaskRequestsComponent implements OnInit {

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

  ngOnInit(): void {
    this.loadRequests();
  }

  loadRequests(): void {
    this.isLoading = true;

    setTimeout(() => {
      this.rows = [];  
      this.totalItems = this.rows.length;
      this.isLoading = false;
    }, 500);
  }

  refresh(): void {
    this.loadRequests();
  }

  onPageChange(page: number) {
    this.page = page;
    this.loadRequests();
  }
}
