import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { RoleService } from 'app/core/services/role.service';
import { RoleAddEdit } from 'app/core/models/roles/role';
import { FormFieldConfig } from 'app/core/models/form-field-config';
import { EmployeeService } from 'app/core/services/employee.service';

@Component({
  selector: 'app-role-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './role-create-update.component.html'
})
export class RoleCreateUpdateComponent implements OnInit {

  @Input() isEdit = false;
  @Input() roleId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  title = 'ROLE.TITLE';
  breadcrumbs = ['HOME', 'ROLES'];
  activeitem = 'ROLE.CREATE';

  formGroup!: FormGroup;
  formConfig: FormFieldConfig[] = [];

  private employeeTypeOptions: { label: string; value: number }[] = [];

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService,
    private employeeService: EmployeeService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.buildFormConfig(false);
    this.loadEmployeeTypes().then(() => {
      if (this.isEdit && this.roleId) {
        this.loadRole();
      }
    });

    this.formGroup.get('requiresEmployeeTypeScope')?.valueChanges.subscribe(required => {
      this.toggleEmployeeTypeField(!!required);
    });
  }

  initForm() {
    this.formGroup = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: [''],
      requiresBranchScope: [false],
      requiresEmployeeTypeScope: [false],
      employeeTypeId: [null],
      canBeBranchManager: [false]
    });
  }

  private buildFormConfig(showEmployeeType: boolean) {
    const fields: FormFieldConfig[] = [
      {
        type: 'input',
        label: 'ROLE.NAME',
        name: 'name',
        validations: { required: true, maxlength: 100 }
      },
      {
        type: 'textarea',
        label: 'ROLE.DESCRIPTION',
        name: 'description'
      },
      {
        type: 'checkbox',
        label: 'ROLE.REQUIRES_BRANCH_SCOPE',
        name: 'requiresBranchScope',
        defaultValue: false
      },
      {
        type: 'checkbox',
        label: 'ROLE.REQUIRES_EMPLOYEE_TYPE_SCOPE',
        name: 'requiresEmployeeTypeScope',
        defaultValue: false
      }
    ];

    if (showEmployeeType) {
      fields.push({
        type: 'select',
        label: 'ROLE.ROLE_EMPLOYEE_TYPE',
        selectType: 'simple',
        name: 'employeeTypeId',
        options: this.employeeTypeOptions,
        validations: { required: true }
      });
    }

    fields.push({
      type: 'checkbox',
      label: 'ROLE.CAN_BE_BRANCH_MANAGER',
      name: 'canBeBranchManager',
      defaultValue: false
    });

    this.formConfig = fields;
  }

  private toggleEmployeeTypeField(required: boolean) {
    const control = this.formGroup.get('employeeTypeId');
    if (required) {
      control?.setValidators([Validators.required]);
    } else {
      control?.clearValidators();
      control?.setValue(null, { emitEvent: false });
    }
    control?.updateValueAndValidity({ emitEvent: false });
    this.buildFormConfig(required);
  }

  private loadEmployeeTypes(): Promise<void> {
    return new Promise(resolve => {
      this.employeeService.getFunctionCodes().subscribe({
        next: res => {
          this.employeeTypeOptions = (res.data ?? []).map(x => ({
            label: x.name,
            value: x.id
          }));
          resolve();
        },
        error: () => resolve()
      });
    });
  }

  loadRole() {
    if (!this.roleId) return;

    this.roleService.getById(this.roleId).subscribe(res => {
      const role = res.data;
      const requiresType = role.requiresEmployeeTypeScope ?? false;
      this.formGroup.patchValue({
        name: role.name,
        description: role.description,
        requiresBranchScope: role.requiresBranchScope ?? false,
        requiresEmployeeTypeScope: requiresType,
        employeeTypeId: role.employeeTypeId ?? null,
        canBeBranchManager: role.canBeBranchManager ?? false
      });
      this.toggleEmployeeTypeField(requiresType);
    });
  }

  onSubmit(formValue: RoleAddEdit) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    const payload: RoleAddEdit = {
      ...formValue,
      employeeTypeId: formValue.requiresEmployeeTypeScope
        ? formValue.employeeTypeId
        : null
    };

    if (this.isEdit && this.roleId) {
      this.roleService.update(this.roleId, payload).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('ROLE.UPDATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    } else {
      this.roleService.create(payload).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('ROLE.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    }
  }
}
