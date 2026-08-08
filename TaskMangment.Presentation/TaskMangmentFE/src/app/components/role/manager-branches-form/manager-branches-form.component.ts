import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { FormFieldConfig } from 'app/core/models/form-field-config';
import { RoleAssignmentService } from 'app/core/services/role-assignment.service';
import { EmployeeService } from 'app/core/services/employee.service';
import { EnumItemDto } from 'app/core/models/employee/employee';

@Component({
  selector: 'app-manager-branches-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, GenericFormComponent],
  templateUrl: './manager-branches-form.component.html'
})
export class ManagerBranchesFormComponent implements OnInit {
  @Input() isEdit = false;
  @Input() managerId: number | null = null;
  @Input() selectedBranchIds: number[] = [];
  /** Kept for parent compatibility — branch coverage is via Area page now. */
  @Input() requiresBranchScope = false;
  @Input() requiresEmployeeTypeScope = false;

  @Output() formSubmitted = new EventEmitter<{
    managerId: number | null;
    employeeTypeId?: number;
    branchIds: number[];
  }>();

  formGroup!: FormGroup;
  private employeeTypes: EnumItemDto[] = [];

  title = 'ROLE.MANAGER_SCOPE';
  breadcrumbs = ['HOME'];
  activeitem = 'ROLE.MANAGER_SCOPE';

  formConfig: FormFieldConfig[] = [];

  constructor(
    private fb: FormBuilder,
    private toastr: ToastrService,
    private translate: TranslateService,
    private roleAssignmentService: RoleAssignmentService,
    private employeeService: EmployeeService
  ) {}

  ngOnInit() {
    this.buildFormConfig();
    this.initForm();

    if (this.requiresEmployeeTypeScope) {
      this.loadEmployeeTypes();
    }
  }

  private buildFormConfig() {
    const fields: FormFieldConfig[] = [];

    if (this.requiresEmployeeTypeScope) {
      fields.push({
        type: 'select',
        label: 'EMPLOYEE.FUNCTION_CODE',
        selectType: 'simple',
        name: 'employeeTypeId',
        options: [],
        validations: { required: true }
      });
    }

    this.formConfig = fields;
  }

  initForm() {
    const group: Record<string, any> = {};

    if (this.requiresEmployeeTypeScope) {
      group['employeeTypeId'] = [null, [Validators.required]];
    }

    this.formGroup = this.fb.group(group);
  }

  loadEmployeeTypes() {
    this.employeeService.getFunctionCodes().subscribe({
      next: (res) => {
        this.employeeTypes = res.data ?? [];
        const options = this.employeeTypes.map((x) => ({
          label: x.name,
          value: x.id
        }));
        const field = this.formConfig.find(f => f.name === 'employeeTypeId');
        if (field) field.options = options;
        this.formConfig = [...this.formConfig];

        if (this.managerId) {
          this.loadManagerData();
        }
      },
      error: () => {
        this.toastr.error(this.translate.instant('COMMON.LOADING_FAILED'));
      }
    });
  }

  private loadManagerData() {
    if (!this.managerId) return;

    this.roleAssignmentService.getManagerBranches(this.managerId).subscribe({
      next: (res) => {
        const dto = res.data;
        if (this.requiresEmployeeTypeScope) {
          const employeeTypeId = dto.employeeTypeId ?? dto.functionCode ?? null;
          this.formGroup.patchValue({ employeeTypeId }, { emitEvent: false });
        }
      },
      error: () => {
        this.toastr.error(this.translate.instant('COMMON.LOADING_FAILED'));
      }
    });
  }

  onSubmit(formValue: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    this.formSubmitted.emit({
      managerId: this.managerId,
      employeeTypeId: formValue.employeeTypeId,
      branchIds: []
    });
  }
}
