import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GenericTableComponent } from '../../shared/components/generic-table/generic-table.component'; 
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-employee-management',
  standalone: true,
  imports: [CommonModule, FormsModule, GenericTableComponent, TranslateModule],  // Add GenericTableComponent and PageHeaderComponent
  templateUrl: './employee-management.component.html',
  styleUrls: ['./employee-management.component.scss']
})
export class EmployeeManagementComponent {
  title = 'Employee Management';
  breadcrumbs = ['Home', 'Employee Management'];
  activeitem = 'Employee Management';

  columns = [
    { key: 'id', label: 'ID' },
    { key: 'name', label: 'Name' },
    { key: 'position', label: 'Position' },
    { key: 'department', label: 'Department' }
  ];

  employees = [
    { id: 1, name: 'John Doe', position: 'Developer', department: 'IT' },
    { id: 2, name: 'Jane Smith', position: 'Designer', department: 'Marketing' },
    { id: 3, name: 'Sam Johnson', position: 'Manager', department: 'HR' }
  ];

  selectedEmployee: any = null;

  constructor(private router: Router) {}

  navigateToForm() {
    this.router.navigate(['/employee-form']);
  }

  onEdit(employee: any) {
    this.selectedEmployee = { ...employee };
    this.navigateToForm(); // Navigate to form when editing
  }

  onDelete(employee: any) {
    this.employees = this.employees.filter(e => e.id !== employee.id);
  }
}
