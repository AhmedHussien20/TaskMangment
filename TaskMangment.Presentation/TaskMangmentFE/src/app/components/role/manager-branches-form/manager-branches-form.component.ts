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
  /** From the role being assigned — drives which fields appear. */
  @Input() requiresBranchScope = false;
  @Input() requiresEmployeeTypeScope = false;

  @Output() formSubmitted = new EventEmitter<{
    managerId: number | null;
    employeeTypeId?: number;
    branchIds: number[];
  }>();

  formGroup!: FormGroup;
  private employeeTypes: EnumItemDto[] = [];

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
      this.watchEmployeeType();
      this.loadEmployeeTypes();
    } else if (this.managerId) {
      this.loadManagerData();
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

    if (this.requiresBranchScope) {
      const branchOnly = !this.requiresEmployeeTypeScope;
      fields.push({
        type: 'select',
        label: 'EMPLOYEE.BRANCH',
        selectType: 'simple',
        name: 'branchIds',
        multiple: true,
        options: [],
        validations: branchOnly ? { required: true } : { required: false },
        disabled: !branchOnly,
        // @ts-ignore
        hidden: !branchOnly
      } as FormFieldConfig);
    }

    this.formConfig = fields;
  }

  initForm() {
    const group: Record<string, any> = {};

    if (this.requiresEmployeeTypeScope) {
      group['employeeTypeId'] = [null, [Validators.required]];
    }

    if (this.requiresBranchScope) {
      const branchOnly = !this.requiresEmployeeTypeScope;
      group['branchIds'] = [
        { value: [], disabled: !branchOnly },
        branchOnly ? [Validators.required] : []
      ];
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

  private watchEmployeeType() {
    const ctrl = this.formGroup.get('employeeTypeId');
    if (!ctrl || !this.requiresBranchScope) return;

    ctrl.valueChanges.subscribe(val => this.toggleBranches(val));
  }

  private loadManagerData() {
    if (!this.managerId) return;

    this.roleAssignmentService.getManagerBranches(this.managerId).subscribe({
      next: (res) => {
        const dto = res.data;

        if (this.requiresEmployeeTypeScope) {
          const employeeTypeId = dto.employeeTypeId ?? dto.functionCode ?? null;
          this.formGroup.patchValue({ employeeTypeId }, { emitEvent: false });
          if (this.requiresBranchScope) {
            this.toggleBranches(employeeTypeId, true);
          }
        }

        if (this.requiresBranchScope) {
          const branchOptions = (dto.branchLookupDtos ?? []).map((b: any) => ({
            label: b.name,
            value: b.id
          }));
          const branchField = this.formConfig.find(x => x.name === 'branchIds');
          if (branchField) branchField.options = branchOptions;

          this.formGroup.patchValue(
            { branchIds: dto.branchIds ?? [] },
            { emitEvent: false }
          );
          this.formConfig = [...this.formConfig];
        }
      },
      error: () => {
        this.toastr.error(this.translate.instant('COMMON.LOADING_FAILED'));
      }
    });
  }

  private toggleBranches(employeeTypeId: any, fromLoad = false) {
    if (!this.requiresBranchScope || !this.requiresEmployeeTypeScope) return;

    const selected = this.employeeTypes.find(t => Number(t.id) === Number(employeeTypeId));
    const showBranches = selected?.seesAllTypesInBranchScope === true;

    const branchesField = this.formConfig.find(f => f.name === 'branchIds');
    const branchesCtrl = this.formGroup.get('branchIds');
    if (!branchesField || !branchesCtrl) return;

    if (showBranches) {
      branchesField.disabled = false;
      // @ts-ignore
      branchesField.hidden = false as any;
      branchesField.validations = { ...(branchesField.validations ?? {}), required: true };
      branchesCtrl.enable({ emitEvent: false });
      branchesCtrl.setValidators([Validators.required]);
      branchesCtrl.updateValueAndValidity({ emitEvent: false });
      this.formConfig = [...this.formConfig];

      if (!fromLoad && (!branchesField.options || branchesField.options.length === 0)) {
        this.loadManagerData();
      }
    } else {
      branchesCtrl.reset([], { emitEvent: false });
      branchesCtrl.clearValidators();
      branchesCtrl.disable({ emitEvent: false });
      branchesCtrl.updateValueAndValidity({ emitEvent: false });
      branchesField.disabled = true;
      // @ts-ignore
      branchesField.hidden = true as any;
      branchesField.validations = { ...(branchesField.validations ?? {}) };
      delete (branchesField.validations as any).required;
      this.formConfig = [...this.formConfig];
    }
  }

  onSubmit() {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    const v = this.formGroup.getRawValue();
    const branchIds = (v.branchIds ?? [])
      .map((x: any) => Number(x))
      .filter((n: number) => !Number.isNaN(n));

    const payload: {
      managerId: number | null;
      employeeTypeId?: number;
      branchIds: number[];
    } = {
      managerId: this.managerId,
      branchIds
    };

    if (this.requiresEmployeeTypeScope && v.employeeTypeId != null) {
      payload.employeeTypeId = Number(v.employeeTypeId);
    }

    this.formSubmitted.emit(payload);
  }
}
