import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { RoleService } from 'app/core/services/role.service';
import { RoleAddEdit } from 'app/core/models/roles/role';

interface FormFieldConfig {
  type: 'input' | 'textarea';
  label: string;
  name: string;
  validations?: any;
}

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

  formConfig: FormFieldConfig[] = [
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
    }
  ];

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initForm();

    if (this.isEdit && this.roleId) {
      this.loadRole();
    }
  }

  initForm() {
    this.formGroup = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(100)]],
      description: ['']
    });
  }

  loadRole() {
    if (!this.roleId) return;

    this.roleService.getById(this.roleId).subscribe(res => {
      const role = res.data;

      this.formGroup.patchValue({
        name: role.name,
        description: role.description
      });
    });
  }

  onSubmit(formValue: RoleAddEdit) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    // EDIT
    if (this.isEdit && this.roleId) {
      this.roleService.update(this.roleId, formValue).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('ROLE.UPDATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    }
    // CREATE
    else {
      this.roleService.create(formValue).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('ROLE.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    }
  }
}
