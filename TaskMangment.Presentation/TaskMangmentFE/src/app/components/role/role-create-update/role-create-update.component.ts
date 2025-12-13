import { Component, EventEmitter, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';

import { RoleService } from 'app/core/services/role.service';

import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormFieldConfig } from 'app/core/models/form-field-config';

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

  @Output() formSubmitted = new EventEmitter<void>();

  title = 'ROLE.CREATE_TITLE';
  breadcrumbs = ['HOME', 'ROLES'];
  activeitem = 'ROLE.CREATE_TITLE';

  formGroup!: FormGroup;

  formConfig: FormFieldConfig[] = [
    { 
      type: 'input', 
      label: 'ROLE.NAME', 
      name: 'name', 
      validations: { required: true, maxlength: 100 }, 
      defaultValue: '' 
    },
    { 
      type: 'textarea', 
      label: 'ROLE.DESCRIPTION', 
      name: 'description', 
      validations: { maxlength: 500 }, 
      defaultValue: '' 
    },
    { 
      type: 'input', 
      inputType: 'number',
      label: 'ROLE.COMPANY_ID', 
      name: 'companyId', 
      validations: { required: false }, 
      defaultValue: '' 
    }
  ];

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) { }

  ngOnInit() {
    this.initForm();
  }

  initForm() {
    this.formGroup = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      companyId: [null]
    });
  }

  onSubmit(formValue: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    this.roleService.create(this.formGroup.value).subscribe({
      next: (res) => {
        this.toastr.success(this.translate.instant('ROLE.CREATE_SUCCESS'));
        this.formSubmitted.emit();
      },
      error: () => {
        this.toastr.error(this.translate.instant('ROLE.CREATE_FAILED'));
      }
    });
  }
}