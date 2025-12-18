import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';
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
  columns = [
    { key: 'employeeName', label: 'EMPLOYEE.NAME' },
    { key: 'role', label: 'EMPLOYEE.ROLE' },
    { key: 'status', label: 'TASK.STATUS' }
  ];

  totalItems = 0;
  private sub!: Subscription;

  constructor(
    private refreshService: TaskDetailsRefreshService,
    private taskService: TaskService
  ) {}

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
          this.rows = res.data;
          this.totalItems = this.rows.length;
        } else {
          this.rows = [];
          this.totalItems = 0;
        }
      },
      error: err => {
        console.error('Failed to load assigned employees', err);
        this.rows = [];
        this.totalItems = 0;
      }
    });
  }
}
