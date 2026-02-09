import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { FormFieldConfig } from 'app/core/models/form-field-config';
import { RoleAssignmentService } from 'app/core/services/role-assignment.service';
import { EmployeeService } from 'app/core/services/employee.service';

@Component({
  selector: 'app-manager-branches-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, GenericFormComponent],
  templateUrl: './manager-branches-form.component.html'
})
export class ManagerBranchesFormComponent implements OnInit {
  @Input() isEdit: boolean = false;

  @Input() managerId: number | null = null;
  @Input() selectedBranchIds: number[] = [];

  @Output() formSubmitted = new EventEmitter<{
    managerId: number | null;
    functionCode: number;
    branchIds: number[];
  }>();

  title = 'ROLE.LEVELS.BRANCHES_MANAGER';
  breadcrumbs = ['HOME'];
  activeitem = 'ROLE.LEVELS.MANAGER_BRANCHES';

  formGroup!: FormGroup;

  // ✅ functionCode select + branchIds (hidden by default)
  formConfig: FormFieldConfig[] = [
    {
      type: 'select',
      label: 'EMPLOYEE.FUNCTION_CODE',
      selectType: 'simple',
      name: 'functionCode',
      options: [],
      validations: { required: true }
    },
    {
      type: 'select',
      label: 'EMPLOYEE.BRANCH',
      selectType: 'simple',
      name: 'branchIds',
      multiple: true,
      options: [],
      validations: { required: false },
      disabled: true,
      // @ts-ignore
      hidden: true
    }
  ];

  constructor(
    private fb: FormBuilder,
    private toastr: ToastrService,
    private translate: TranslateService,
    private roleAssignmentService: RoleAssignmentService,
    private employeeService: EmployeeService
  ) {}

  ngOnInit() {
    this.initForm();
    this.loadFunctionCodes();
    this.watchFunctionCode();

    if (this.managerId) {
      this.loadManagerData(); 
    }
  }

  initForm() {
    this.formGroup = this.fb.group({
      functionCode: [null, [Validators.required]],
      branchIds: [{ value: [], disabled: true }]
    });
  }

  loadFunctionCodes() {
    this.employeeService.getFunctionCodes().subscribe({
      next: (res) => {
        const functionCodes = res.data ?? [];

        const options = functionCodes.map((x: any) => ({
          label: x.name,
          value: x.id
        }));

        const field = this.formConfig.find(f => f.name === 'functionCode');
        if (field) field.options = options;

        this.formConfig = [...this.formConfig];
      },
      error: () => {
        this.toastr.error(this.translate.instant('COMMON.LOADING_FAILED'));
      }
    });
  }

  private watchFunctionCode() {
    const ctrl = this.formGroup.get('functionCode');
    if (!ctrl) return;

    ctrl.valueChanges.subscribe(val => {
      this.toggleBranches(val);
    });
  }
  private loadManagerData() {
    if (!this.managerId) return;

    this.roleAssignmentService.getManagerBranches(this.managerId).subscribe({
      next: (res) => {
        const dto = res.data;

        this.formGroup.patchValue(
          { functionCode: dto.functionCode },
          { emitEvent: false }
        );

        this.toggleBranches(dto.functionCode, true);

        const branchOptions = (dto.branchLookupDtos ?? []).map((b: any) => ({
          label: b.name,
          value: b.id
        }));

        const branchField = this.formConfig.find(x => x.name === 'branchIds');
        if (branchField) branchField.options = branchOptions;

        const selected = (dto.branchIds ?? []);
        this.formGroup.patchValue({ branchIds: selected }, { emitEvent: false });

        this.formConfig = [...this.formConfig];
      },
      error: () => {
        this.toastr.error(this.translate.instant('COMMON.LOADING_FAILED'));
      }
    });
  }


  private toggleBranches(functionCodeId: any, fromLoad: boolean = false) {
    const isOperations = Number(functionCodeId) === 1;

    const branchesField = this.formConfig.find(f => f.name === 'branchIds');
    const branchesCtrl = this.formGroup.get('branchIds');

    if (!branchesField || !branchesCtrl) return;

    if (isOperations) {
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

    const functionCode = Number(v.functionCode);

    const branchIds = (v.branchIds ?? [])
      .map((x: any) => Number(x))
      .filter((n: number) => !Number.isNaN(n));

    this.formSubmitted.emit({
      managerId: this.managerId,
      functionCode,
      branchIds
    });
  }
}
