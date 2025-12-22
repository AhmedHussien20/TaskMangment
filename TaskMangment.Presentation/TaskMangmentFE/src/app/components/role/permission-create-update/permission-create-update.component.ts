import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';

import { PermissionService } from 'app/core/services/permission.service';

import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormFieldConfig } from 'app/core/models/form-field-config';

@Component({
  selector: 'app-permission-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './permission-create-update.component.html'
})
export class PermissionCreateUpdateComponent implements OnInit {

  @Input() isEdit: boolean = false;
  @Input() permissionId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  title = 'PERMISSION.TITLE';
  breadcrumbs = ['HOME', 'PERMISSIONS'];
  activeitem = 'PERMISSION.CREATE';

  formGroup!: FormGroup;

  formConfig: FormFieldConfig[] = [
    { 
      type: 'input', 
      label: 'PERMISSION.CODE', 
      name: 'code', 
      validations: { required: true, maxlength: 100 }, 
      defaultValue: '' 
    },
    { 
      type: 'input', 
      label: 'PERMISSION.NAME', 
      name: 'name', 
      validations: { required: true, maxlength: 200 }, 
      defaultValue: '' 
    },
    { 
      type: 'textarea', 
      label: 'PERMISSION.DESCRIPTION', 
      name: 'description', 
      validations: { maxlength: 500 }, 
      defaultValue: '' 
    }
  ];

  constructor(
    private fb: FormBuilder,
    private permissionService: PermissionService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) { }

  ngOnInit() {
    this.initForm();
    if (this.isEdit && this.permissionId) {
      this.loadPermission();
    }
  }

  initForm() {
    this.formGroup = this.fb.group({
      code: ['', Validators.required],
      name: ['', Validators.required],
      description: ['']
    });
  }

  loadPermission() {
    if (!this.permissionId) return;

    // Note: الـ Backend ماعندهاش GetById للـ Permission
    // محتاج تضيفها في الـ Service
    this.toastr.warning('GetById not implemented in backend');
  }

  onSubmit(formValue: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    // UPDATE
    if (this.isEdit && this.permissionId) {
      this.permissionService.update(this.permissionId, this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('PERMISSION.UPDATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    }

    // CREATE
    else {
      this.permissionService.create(this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('PERMISSION.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      
      });
    }
  }
}