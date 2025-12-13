import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';

import { BranchService } from 'app/core/services/branch.service';
import { AreaService } from 'app/core/services/area.service';
import { EmployeeService } from 'app/core/services/employee.service';

import { Employee } from 'app/core/models/employee/employee';
import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

export interface SelectOption {
  label: string;
  value: number;
}

interface FormFieldConfig {
  type: 'input' | 'select' | 'textarea';
  label: string;
  name: string;
  validations?: any;
  defaultValue?: any;
  options?: SelectOption[];
}

@Component({
  selector: 'app-branch-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './branch-create-update.component.html'
})
export class BranchCreateUpdateComponent implements OnInit {

  @Input() isEdit: boolean = false;
  @Input() branchId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  employees: Employee[] = [];
  areas: any[] = [];

  title = 'BRANCH.TITLE';
  breadcrumbs = ['HOME', 'BRANCHES'];
  activeitem = 'BRANCH.CREATE';

  formGroup!: FormGroup;

  formConfig: FormFieldConfig[] = [
    { type: 'input', label: 'BRANCH.NAME', name: 'name', validations: { required: true, maxlength: 200 }, defaultValue: '' },
    { type: 'input', label: 'BRANCH.ADDRESS', name: 'address', validations: { maxlength: 500 }, defaultValue: '' },
    { type: 'input', label: 'BRANCH.PHONE', name: 'phone', validations: { maxlength: 50 }, defaultValue: '' },
    { type: 'input', label: 'BRANCH.MOBILE', name: 'mobile', validations: { maxlength: 50 }, defaultValue: '' },
    { type: 'input', label: 'BRANCH.FAX', name: 'fax', validations: { maxlength: 50 }, defaultValue: '' },
    { type: 'input', label: 'BRANCH.EMAIL', name: 'email', validations: { email: true, maxlength: 200 }, defaultValue: '' },
    { type: 'select', label: 'BRANCH.AREA', name: 'areaId', options: [], validations: { required: true } },
    { type: 'select', label: 'BRANCH.MANAGER', name: 'managerId', options: [], validations: { required: false } },
    { type: 'select', label: 'BRANCH.RESPONSIBLE', name: 'responsibleId', options: [], validations: { required: false } }
  ];

  constructor(
    private fb: FormBuilder,
    private branchService: BranchService,
    private areaService: AreaService,
    private employeeService: EmployeeService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) { }

  ngOnInit() {
    this.initForm();
    this.loadEmployees();
    this.loadAreas();
    if (this.isEdit && this.branchId) {
      this.loadBranch();
    }
  }

  initForm() {
    this.formGroup = this.fb.group({
      name: ['', Validators.required],
      address: [''],
      phone: [''],
      mobile: [''],
      fax: [''],
      email: ['', Validators.email],

      areaId: [null, Validators.required],
      managerId: [null],
      responsibleId: [null],
    });
  }

  loadBranch() {
    if (!this.branchId) return;

    this.branchService.getById(this.branchId).subscribe(res => {
      const b = res.data;

      this.formGroup.patchValue({
        name: b.name,
        address: b.address,
        phone: b.phone,
        mobile: b.mobile,
        fax: b.fax,
        email: b.email,
        areaId: b.areaId,
        managerId: b.managerId,
        responsibleId: b.responsibleId
      });
    });
  }

  loadAreas() {
    const req = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 500,
      sortColumn: 'Id',
      sortDirection: 'ASC'
    };

    this.areaService.getAll(req).subscribe(res => {
      const list = res.data.data;
      const field = this.formConfig.find(x => x.name === 'areaId');

      if (field) {
        field.options = list.map(a => ({
          label: a.name,
          value: a.id
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
      const list = res.data.data;

      const managerField = this.formConfig.find(x => x.name === 'managerId');
      const responsibleField = this.formConfig.find(x => x.name === 'responsibleId');

      if (managerField) {
        managerField.options = list.map((emp: Employee) => ({
          label: emp.fullName,
          value: emp.id
        }));
      }

      if (responsibleField) {
        responsibleField.options = list.map((emp: Employee) => ({
          label: emp.fullName,
          value: emp.id
        }));
      }
    });
  }

  onSubmit(formValue: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    // UPDATE
    if (this.isEdit && this.branchId) {
      this.branchService.update(this.branchId, this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('BRANCH.UPDATE_SUCCESS'));
          this.formSubmitted.emit();
        },
        error: () => {
          this.toastr.error(this.translate.instant('BRANCH.UPDATE_FAILED'));
        }
      });
    }

    // CREATE
    else {
      this.branchService.create(this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('BRANCH.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        },
        error: () => {
          this.toastr.error(this.translate.instant('BRANCH.CREATE_FAILED'));
        }
      });
    }
  }
}
