import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { GenericFormComponent } from '../../shared/components/generic-form/generic-form.component';
import { Router } from '@angular/router';

@Component({
  selector: 'app-employee-form',
  standalone: true,
  imports: [CommonModule, FormsModule, GenericFormComponent],
  templateUrl: './employee-form.component.html',
  styleUrls: ['./employee-form.component.scss']
})
export class EmployeeFormComponent {
  title = 'Employee Form';
  breadcrumbs = ['Home', 'Employee Management', 'Employee Form'];
  activeitem = 'Employee Form';

  formConfig = [
    { type: 'input', label: 'Name', name: 'name', validations: { required: true, minlength: 3, maxlength: 50 } },
    { type: 'input', label: 'Position', name: 'position', validations: { required: true, minlength: 3, maxlength: 50 } },
    { type: 'input', label: 'Department', name: 'department', validations: { required: true, minlength: 3, maxlength: 50 } }
  ];

  constructor(private router: Router) {}

  onSubmit(formValue: any) {
    // Handle form submission logic here
    console.log('Form submitted:', formValue);
    this.router.navigate(['/employee-management']); // Navigate back to employee management page
  }
}
