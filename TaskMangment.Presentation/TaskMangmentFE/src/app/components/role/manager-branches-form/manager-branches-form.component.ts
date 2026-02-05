import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { FormFieldConfig } from 'app/core/models/form-field-config';
import { BranchService } from 'app/core/services/branch.service';
import { RoleAssignmentService } from 'app/core/services/role-assignment.service';

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

  @Output() formSubmitted = new EventEmitter<{ managerId: number | null; branchIds: number[] }>();

  title = 'ROLE.LEVELS.BRANCHES_MANAGER';
  breadcrumbs = ['HOME'];
  activeitem = 'ROLE.LEVELS.MANAGER_BRANCHES';

  formGroup!: FormGroup;

  formConfig: FormFieldConfig[] = [
    {
      type: 'select',
      label: 'EMPLOYEE.BRANCH',   
      selectType: 'simple',
      name: 'branchIds',
      multiple: true,            
      options: [],
      validations: { required: true }
    }
  ];

  constructor(
    private fb: FormBuilder,
    private branchService: BranchService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private roleAssignmentService: RoleAssignmentService,

  ) {}

  ngOnInit() {
  this.initForm();
  this.loadBranches();
}


  initForm() {
    this.formGroup = this.fb.group({
      // ✅ نخزن strings لأن options.value عندك string
      branchIds: [[], [Validators.required]]
    });
  }

  loadBranches() {
  if (!this.managerId) {
    // لو لسه managerId مش جاهز
    const field = this.formConfig.find(x => x.name === 'branchIds');
    if (field) field.options = [];
    this.formGroup.get('branchIds')?.setValue([]);
    return;
  }

  this.roleAssignmentService.getManagerBranches(this.managerId).subscribe({
    next: (res) => {
      const dto = res.data; // GetManagerBranchesDto

      // ✅ options (available + current)
      const options = (dto.branchLookupDtos ?? []).map(b => ({
        label: b.name,
        value: b.id.toString()
      }));

      const field = this.formConfig.find(x => x.name === 'branchIds');
      if (field) field.options = options;

      // ✅ selected للفورم = الفروع الحالية من الـ API (مش من Input)
      const selected = (dto.branchIds ?? []).map(x => x.toString());
      this.formGroup.patchValue({ branchIds: selected });
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

    const branchIds = (formValue.branchIds ?? []).map((x: any) => Number(x)).filter((n: number) => !Number.isNaN(n));

    this.formSubmitted.emit({
      managerId: this.managerId,
      branchIds
    });
  }
}
