import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TaskService } from 'app/core/services/task.service';
import { Subscription } from 'rxjs';
import { TaskAssignedEmployee } from 'app/core/models/task/task';

@Component({
  selector: 'app-task-employees',
  standalone: true,
  imports: [CommonModule, GenericTableComponent],
  templateUrl: './task-employees.component.html'
})
export class TaskEmployeesComponent implements OnInit, OnDestroy {

  @Input() taskId!: number;
  @Input() readonly = false;

  rows: TaskAssignedEmployee[] = [];
  allRows: TaskAssignedEmployee[] = [];
  columns:TableColumn[] = [
    { key: 'employeeName', label: 'EMPLOYEE.NAME' },
    { key: 'role', label: 'EMPLOYEE.ROLE' },
    {
      key: 'status',
      label: 'TASK.STATUS',
      type: 'badge'as const,
      badgeMap: {
        Active: { text: 'TASK.ACTIVE', class: 'bg-success' },
        Inactive: { text: 'TASK.INACTIVE', class: 'bg-secondary' },
      }
    },
    { key: 'isRead', label: 'TASK.READED', type: 'seen' }
  ];


  totalItems = 0;
  page = 1;
  entries = 10;
  private sub!: Subscription;

  constructor(
    private refreshService: TaskDetailsRefreshService,
    private taskService: TaskService
  ) { }

  ngOnInit() {
    this.loadEmployees();

    this.sub = this.refreshService.refresh$
      .subscribe(source => {
        console.log('Employees tab refresh from:', source);
        this.loadEmployees();
      });
  }

  ngOnDestroy() {
    this.sub?.unsubscribe();
  }

  loadEmployees() {
    if (!this.taskId) return;

    this.taskService.getAssignedEmployees(this.taskId).subscribe({
      next: res => {
        if (res.success && res.data) {
          this.allRows = res.data;
          this.totalItems = this.allRows.length;
          this.applyPage();
        } else {
          this.allRows = [];
          this.rows = [];
          this.totalItems = 0;
        }
      },
      error: err => {
        console.error('Failed to load assigned employees', err);
        this.allRows = [];
        this.rows = [];
        this.totalItems = 0;
      }
    });
  }

  private applyPage(): void {
    const start = (this.page - 1) * this.entries;
    this.rows = this.allRows.slice(start, start + this.entries);
  }

  onPageChange(page: number): void {
    this.page = page;
    this.applyPage();
  }

  onEntriesChange(entries: number): void {
    this.entries = entries;
    this.page = 1;
    this.applyPage();
  }
}
