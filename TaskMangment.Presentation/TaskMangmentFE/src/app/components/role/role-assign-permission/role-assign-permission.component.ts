import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { NgSelectModule } from '@ng-select/ng-select';

import { RoleService } from 'app/core/services/role.service';
import { PermissionService } from 'app/core/services/permission.service';

import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormFieldConfig } from 'app/core/models/form-field-config';

@Component({
  selector: 'app-role-assign-permissions',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    NgSelectModule,
    GenericFormComponent
  ],
  templateUrl: './role-assign-permission.component.html'
})
export class RoleAssignPermissionsComponent implements OnInit {

  @Input() roleId?: number;
  @Output() assigned = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  title = 'ROLE.ASSIGN_PERMISSIONS_TITLE';
  breadcrumbs = ['HOME', 'ROLES', 'ASSIGN_PERMISSIONS'];
  activeitem = 'ROLE.ASSIGN_PERMISSIONS';

  formGroup!: FormGroup;
  roles: any[] = [];
  permissions: any[] = [];

  formConfig: FormFieldConfig[] = [
    { 
      type: 'select', 
      label: 'ROLE.SELECT_ROLE', 
      name: 'roleId', 
      options: [],
      validations: { required: true } 
    },
    { 
      type: 'select', 
      label: 'PERMISSION.SELECT_PERMISSIONS', 
      name: 'permissionIds', 
      multiple: true,
      options: [],
      validations: { required: true } 
    }
  ];

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService,
    private permissionService: PermissionService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) { }

  ngOnInit() {
    this.initForm();
    this.loadRoles();
    this.loadPermissions();
  }

  initForm() {
    this.formGroup = this.fb.group({
      roleId: [this.roleId || null, Validators.required],
      permissionIds: [[], Validators.required]
    });
  }

  loadRoles() {
    this.roleService.getRoles().subscribe(res => {
      this.roles = res.data;
      const field = this.formConfig.find(x => x.name === 'roleId');
      if (field) {
        field.options = this.roles.map(r => ({
          label: r.name,
          value: r.id
        }));
      }
    });
  }

  loadPermissions() {
    const req = {
      pageIndex: 1,
      pageSize: 1000,
      sortColumn: 'Id',
      sortDirection: 'ASC'
    };

    this.permissionService.getAll(req).subscribe(res => {
      this.permissions = res.data.data;
      const field = this.formConfig.find(x => x.name === 'permissionIds');
      if (field) {
        field.options = this.permissions.map(p => ({
          label: `${p.code} - ${p.name}`,
          value: p.id
        }));
      }
    });
  }

  onSubmit(formValue: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    const { roleId, permissionIds } = this.formGroup.value;

    this.roleService.assignPermissions(roleId, permissionIds).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('ROLE.PERMISSIONS_ASSIGNED_SUCCESS'));
        this.assigned.emit();
      },
      error: () => {
        this.toastr.error(this.translate.instant('ROLE.PERMISSIONS_ASSIGNED_FAILED'));
      }
    });
  }

  onCancel() {
    this.cancel.emit();
  }
}