import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { EmployeeTypeService } from 'app/core/services/employee-type.service';
import { EmployeeTypeAddEditDto } from 'app/core/models/employee/employee-type.model';

@Component({
  selector: 'app-employee-type-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './employee-type-create-update.component.html'
})
export class EmployeeTypeCreateUpdateComponent implements OnInit {

  @Input() isEdit = false;
  @Input() employeeTypeId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  title = 'EMPLOYEE_TYPE.TITLE';
  activeitem = 'EMPLOYEE_TYPE.TITLE';
  breadcrumbs = ['MENU.HOME', 'MENU.EMPLOYEES', 'EMPLOYEE_TYPE.TITLE'];

  formGroup!: FormGroup;

  formConfig = [
    {
      type: 'input',
      label: 'EMPLOYEE_TYPE.CODE',
      name: 'code',
      validations: { required: true, maxlength: 50 },
      defaultValue: ''
    },
    {
      type: 'input',
      label: 'EMPLOYEE_TYPE.NAME_EN',
      name: 'nameEn',
      validations: { required: true, minlength: 2, maxlength: 100 },
      defaultValue: ''
    },
    {
      type: 'input',
      label: 'EMPLOYEE_TYPE.NAME_AR',
      name: 'nameAr',
      validations: { required: true, minlength: 2, maxlength: 100 },
      defaultValue: ''
    },
    {
      type: 'checkbox',
      label: 'EMPLOYEE_TYPE.SEES_ALL_TYPES',
      name: 'seesAllTypesInBranchScope',
      defaultValue: false
    }
  ];

  constructor(
    private fb: FormBuilder,
    private employeeTypeService: EmployeeTypeService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  ngOnInit() {
    this.initForm();

    if (this.isEdit && this.employeeTypeId) {
      this.loadEmployeeType();
    }
  }

  initForm() {
    this.formGroup = this.fb.group({
      code: ['', [Validators.required, Validators.maxLength(50)]],
      nameEn: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      nameAr: ['', [Validators.required, Validators.minLength(2), Validators.maxLength(100)]],
      seesAllTypesInBranchScope: [false]
    });
  }

  loadEmployeeType() {
    if (!this.employeeTypeId) return;

    this.employeeTypeService.getById(this.employeeTypeId).subscribe(res => {
      const item: EmployeeTypeAddEditDto = res.data;
      this.formGroup.patchValue({
        code: item.code,
        nameEn: item.nameEn,
        nameAr: item.nameAr,
        seesAllTypesInBranchScope: item.seesAllTypesInBranchScope
      });
    });
  }

  onSubmit(_formData: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    if (this.isEdit && this.employeeTypeId) {
      this.employeeTypeService.update(this.employeeTypeId, this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('EMPLOYEE_TYPE.UPDATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    } else {
      this.employeeTypeService.add(this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('EMPLOYEE_TYPE.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    }
  }
}
