import { CommonModule } from "@angular/common";
import { Component, EventEmitter, Input, OnInit, Output } from "@angular/core";
import { FormBuilder, FormGroup, ReactiveFormsModule } from "@angular/forms";
import { TranslateModule, TranslateService } from "@ngx-translate/core";
import { Employee } from "app/core/models/employee/employee";
import { FormFieldConfig } from "app/core/models/form-field-config";
import { BranchService } from "app/core/services/branch.service";
import { DepartmentService } from "app/core/services/department.service";
import { EmployeeService } from "app/core/services/employee.service";
import { GenericFormComponent } from "app/shared/components/generic-form/generic-form.component";
import { Validators } from "ngx-editor";
import { ToastrService } from "ngx-toastr";

@Component({
  selector: 'app-department-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './department-create-update.component.html'
})
export class DepartmentCreateUpdateComponent implements OnInit {

  @Input() isEdit = false;
  @Input() departmentId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  formGroup!: FormGroup;
  branches: any[] = [];
  employees: any[] = [];

  title = 'DEPARTMENT.TITLE';
  breadcrumbs = ['HOME', 'DEPARTMENT'];
  activeitem = 'DEPARTMENT.CREATE';

  formConfig: FormFieldConfig[] = [
    {
      type: 'select',
      label: 'DEPARTMENT.BRANCH',
      name: 'branchId',
      options: [], 
      validations: { required: true },
      defaultValue: ''
    },
    {
      type: 'input',
      label: 'DEPARTMENT.NAME',
      name: 'name',
      validations: { required: true, minlength: 3, maxlength: 200 }
    },
    {
      type: 'select',
      label: 'DEPARTMENT.MANAGER',
      selectType: 'employee',
      name: 'managerEmployeeId',
      options: [],
      validations: { required: true },
      defaultValue: null
    }
  ];

  constructor(
    private fb: FormBuilder,
    private deptService: DepartmentService,
    private branchService: BranchService,
    private employeeService: EmployeeService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.loadBranches();
    this.loadEmployees();
    if (this.isEdit && this.departmentId) 
      this.loadDepartment();
  }

  initForm() {
    this.formGroup = this.fb.group({
      name: ['', [
        Validators.required,
        Validators.maxLength(200)
      ]],
      branchId: [null, Validators.required],

      managerEmployeeId: [null, Validators.required]
    });
  }


  loadBranches() {
    this.branchService.getAll({ searchKey: '', pageIndex: 1, pageSize: 1000, sortColumn: 'Id', sortDirection: 'ASC' })
      .subscribe(res => {
        const list = res.data.data;
        const field = this.formConfig.find(f => f.name === 'branchId');
        if (field) field.options = list.map((b: any) => ({ label: b.name, value: b.id }));
      });
  }

  loadEmployees() {
    this.employeeService.getAll({ searchKey: '', pageIndex: 1, pageSize: 500, sortColumn: 'Id', sortDirection: 'ASC' })
      .subscribe(res => {
        const list = res.data.data;
        const field = this.formConfig.find(f => f.name === 'managerEmployeeId');
        if (field) field.options = list.map((e: Employee) => 
          ({ label: e.fullName, 
            value: e.id,
            mobile: e.mobile,
          email: e.email
           }));
      });
  }

  loadDepartment() {
    this.deptService.getById(this.departmentId!).subscribe(res => {
      const d = res.data;
      this.formGroup.patchValue(d);
    });
  }

  onSubmit(formValue: any) {
    debugger
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();

      this.toastr.error(
        this.translate.instant('FORM.VALIDATION_ERROR'),
        this.translate.instant('FORM.ERROR'),
        {
          timeOut: 3000,
          positionClass: 'toast-top-right',
        }
      );

      return;
    }

    if (this.isEdit) {
      this.deptService.update(this.departmentId!, this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('DEPT.UPDATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    } else {
      this.deptService.create(this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('DEPT.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    }
  }
}
