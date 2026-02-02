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
import { FormFieldConfig } from 'app/core/models/form-field-config';

export interface SelectOption {
  label: string;
  value: number;
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
    { type: 'input', label: 'BRANCH.NAME', name: 'name' },
    { type: 'input', label: 'BRANCH.ADDRESS', name: 'address' },
    {
       type: 'input',
      inputType: 'number',
      label: 'BRANCH.PHONE',
      name: 'phone'},

    { 
      type: 'input', 
      inputType: 'number',
      label: 'BRANCH.MOBILE', 
      name: 'mobile'
    },
    { type: 'input', label: 'BRANCH.FAX', name: 'fax'},

    { 
      type: 'input',
      inputType: 'email',
      label: 'BRANCH.EMAIL', 
      name: 'email'
    },
    { type: 'select', label: 'BRANCH.AREA', selectType: 'simple',name: 'areaId', options: []},
    { type: 'select', label: 'BRANCH.MANAGER',selectType: 'employee',name: 'managerId',
      searchFunction: (searchTerm: string) => {
          const request = {
            searchKey: searchTerm || '',
            pageIndex: 1,
            pageSize: 20, 
            sortColumn: 'Id',
            sortDirection: 'DESC'
          };
          
          return this.employeeService.getAll(request);
        },
       options: []},
    { type: 'select', label: 'BRANCH.RESPONSIBLE',selectType: 'employee',name: 'responsibleId', options: []}
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
    name: ['', [
      Validators.required,
      Validators.maxLength(200)
    ]],

    address: ['', Validators.maxLength(500)],

    phone: ['', [
      Validators.maxLength(50),
Validators.pattern('^\\+?[0-9]+$')
    ]],

    mobile: ['', [
      Validators.maxLength(50),
Validators.pattern('^\\+?[0-9]+$')
    ]],

    fax: ['', Validators.maxLength(50)],

    email: ['', [
      Validators.email,
      Validators.maxLength(200),
      Validators.email
    ]],

    areaId: [null, Validators.required],

    managerId: [null,Validators.required],

    responsibleId: [null,Validators.required],
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
        managerId: b.managerID,
        responsibleId: b.responsibleID
      });
    });
  }

  loadAreas() {
    const req = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 500,
      sortColumn: 'Id',
      sortDirection: 'DESC'
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

 loadEmployees(): void {
  const managerField = this.formConfig.find(f => f.name === 'managerId');
  const responsibleField = this.formConfig.find(f => f.name === 'responsibleId');

  const searchFn = (searchTerm: string, page: number) => {
    const request = {
      searchKey: searchTerm || '',
      pageIndex: page,
      pageSize: 20,
      sortColumn: 'Id',
      sortDirection: 'DESC',
      roleLevel: 70
    };
    return this.employeeService.getAll(request);
  };

  const mapOptions = (res: any) =>
    (res?.data?.data ?? []).map((emp: Employee) => ({
      label: emp.fullName,
      value: emp.id,
      mobile: emp.mobile,
      email: emp.email
    }));

  const loadFirstPage = (field: any, afterLoad?: () => void) => {
    field.isPaginated = true;
    field.searchFunction = searchFn;

    field.searchFunction('', 1).subscribe((res: any) => {
      field.options = mapOptions(res);
      afterLoad?.();
    });
  };

  if (managerField) loadFirstPage(managerField);
  if (responsibleField) loadFirstPage(responsibleField);

  if (this.isEdit && this.branchId) {
    const patch = () => {
      this.branchService.getById(this.branchId!).subscribe(bRes => {
        const branch = bRes?.data;
        this.formGroup.patchValue({
          managerId: branch?.managerID ?? null,
          responsibleId: branch?.responsibleID ?? null,
          areaId: branch?.areaId ?? null
        });
      });
    };

    if (managerField) {
      loadFirstPage(managerField, patch);
      if (responsibleField) loadFirstPage(responsibleField);
    } else if (responsibleField) {
      loadFirstPage(responsibleField, patch);
    }
  }
}

  onSubmit(formValue: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    if (this.isEdit && this.branchId) {
      this.branchService.update(this.branchId, this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('BRANCH.UPDATE_SUCCESS'));
          this.formSubmitted.emit();
        }
     
      });
    }

    // CREATE
    else {
      this.branchService.create(this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('BRANCH.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        }
       
      });
    }
  }
}
