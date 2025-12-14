import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { NgSelectModule } from '@ng-select/ng-select';

import { RoleService } from 'app/core/services/role.service';
import { EmployeeService } from 'app/core/services/employee.service';

import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormFieldConfig } from 'app/core/models/form-field-config';

@Component({
  selector: 'app-role-assign-employee',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    NgSelectModule,
    GenericFormComponent
  ],
  templateUrl: './role-assign-employee.component.html'
})
export class RoleAssignEmployeeComponent implements OnInit {

  @Input() employeeId?: number;
  @Input() roleId?: number;
  @Output() assigned = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  title = 'ROLE.ASSIGN_TO_EMPLOYEE_TITLE';
  breadcrumbs = ['HOME', 'ROLES', 'ASSIGN_TO_EMPLOYEE'];
  activeitem = 'ROLE.ASSIGN_TO_EMPLOYEE';

  formGroup!: FormGroup;
  employees: any[] = [];
  roles: any[] = [];

  formConfig: FormFieldConfig[] = [
    { 
      type: 'select', 
      label: 'ROLE.SELECT_EMPLOYEE', 
      selectType: 'employee',
      name: 'employeeId', 
      options: [],
      validations: { required: true } 
    },
    { 
      type: 'select', 
      label: 'ROLE.SELECT_ROLE', 
      name: 'roleId', 
      options: [],
      validations: { required: true } 
    }
  ];

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService,
    private employeeService: EmployeeService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) { }

  ngOnInit() {
    this.initForm();
    this.loadRoles();
    this.loadEmployees();
  }

  initForm() {
    this.formGroup = this.fb.group({
      employeeId: [this.employeeId || null, Validators.required],
      roleId: [this.roleId || null, Validators.required]
    });
  }

  loadRoles() {
    this.roleService.getRoles().subscribe(res => {
      this.roles = res.data;
      const field = this.formConfig.find(x => x.name === 'roleId');
      if (field) {
        field.options = this.roles.map(r => ({
          label: r.name,
          value: r.id
        }));
      }
    });
  }

  loadEmployees() {
    const req = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 1000,
      sortColumn: 'Id',
      sortDirection: 'ASC'
    };

    this.employeeService.getAll(req).subscribe(res => {
      this.employees = res.data.data;
      const field = this.formConfig.find(x => x.name === 'employeeId');
      if (field) {
        field.options = this.employees.map(e => ({
          label: e.fullName,
          value: e.id,
          mobile: e.mobile,
          email: e.email
        }));
      }


    });
  }

  onSubmit(formValue: any) {
      console.log(this.formGroup.value);
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    const { employeeId, roleId } = this.formGroup.value;

    this.roleService.assignToEmployee(employeeId, roleId).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('ROLE.ASSIGNED_TO_EMPLOYEE_SUCCESS'));
        this.assigned.emit();
      },
      error: () => {
        this.toastr.error(this.translate.instant('ROLE.ASSIGNED_TO_EMPLOYEE_FAILED'));
      }
    });
  }

  onCancel() {
    this.cancel.emit();
  }
}