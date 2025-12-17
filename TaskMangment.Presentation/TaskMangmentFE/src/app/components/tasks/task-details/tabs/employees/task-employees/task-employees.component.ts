import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-task-employees',
  standalone: true,
  imports: [CommonModule, GenericTableComponent],
  templateUrl: './task-employees.component.html'
})
export class TaskEmployeesComponent implements OnInit {

  @Input() taskId!: number;
  @Input() readonly = false;
  private sub!: Subscription;

  rows: any[] = [];
  columns = [
    { key: 'name', label: 'EMPLOYEE.NAME' },
    { key: 'role', label: 'EMPLOYEE.ROLE' },
    { key: 'status', label: 'STATUS' }
  ];
  constructor(
    private refreshService: TaskDetailsRefreshService
  ) {}
  totalItems = 0;

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
    this.rows = [
      { id: 1, name: 'Ahmed', role: 'Developer', status: 'Active' },
      { id: 2, name: 'Sara', role: 'QA', status: 'Inactive' }
    ];

    this.totalItems = this.rows.length;
  }
}
