import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { RoleService } from 'app/core/services/role.service';
import { NotificationScope } from 'app/core/models/roles/role';
import { FormFieldConfig } from 'app/core/models/form-field-config';

@Component({
  selector: 'app-role-notification-edit',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './role-notification-edit.component.html'
})
export class RoleNotificationEditComponent implements OnInit {

  @Input() roleId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  title = 'ROLE.NOTIFY_FROM_EMPLOYEES';
  breadcrumbs = ['HOME', 'ROLES'];
  activeitem = 'ROLE.NOTIFY_FROM_EMPLOYEES';

  formGroup!: FormGroup;
  formConfig: FormFieldConfig[] = [];

  private roleOptions: { value: number; label: string }[] = [];

  constructor(
    private fb: FormBuilder,
    private roleService: RoleService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.formGroup = this.fb.group({
      notificationScope: [NotificationScope.None],
      notifyFromRoleIds: [[]]
    });
    this.loadRoleOptions();
  }

  private buildFormConfig() {
    this.formConfig = [
      {
        type: 'select',
        label: 'ROLE.NOTIFICATION_SCOPE',
        name: 'notificationScope',
        selectType: 'simple',
        options: [
          { value: NotificationScope.None, label: this.translate.instant('ROLE.NOTIFY_SCOPE_NONE') },
          { value: NotificationScope.Branch, label: this.translate.instant('ROLE.NOTIFY_SCOPE_BRANCH') },
          { value: NotificationScope.Area, label: this.translate.instant('ROLE.NOTIFY_SCOPE_AREA') },
          { value: NotificationScope.Company, label: this.translate.instant('ROLE.NOTIFY_SCOPE_COMPANY') }
        ]
      },
      {
        type: 'select',
        label: 'ROLE.NOTIFY_FROM_ROLES',
        name: 'notifyFromRoleIds',
        selectType: 'simple',
        multiple: true,
        options: this.roleOptions
      }
    ];
  }

  loadRoleOptions() {
    this.roleService.getAll({
      pageIndex: 1,
      pageSize: 500,
      sortColumn: 'Name',
      sortDirection: 'ASC',
      searchKey: ''
    } as any).subscribe({
      next: (res) => {
        const rows = res.data?.data ?? [];
        this.roleOptions = rows
          .filter((r: any) => !this.roleId || r.id !== this.roleId)
          .map((r: any) => ({ value: r.id, label: r.name }));
        this.buildFormConfig();
        this.loadRole();
      },
      error: () => {
        this.roleOptions = [];
        this.buildFormConfig();
        this.loadRole();
      }
    });
  }

  loadRole() {
    if (!this.roleId) return;

    this.roleService.getById(this.roleId).subscribe(res => {
      const role = res.data;
      this.formGroup.patchValue({
        notificationScope: role.notificationScope ?? NotificationScope.None,
        notifyFromRoleIds: role.notifyFromRoleIds ?? []
      });
    });
  }

  onSubmit(formValue: { notificationScope: number; notifyFromRoleIds: number[] }) {
    if (!this.roleId) return;

    const notifyFromRoleIds = formValue.notifyFromRoleIds ?? [];
    const notificationScope = Number(formValue.notificationScope ?? 0);

    if (notifyFromRoleIds.length > 0 &&
        (!notificationScope || notificationScope === NotificationScope.None)) {
      this.toastr.error(this.translate.instant('ROLE.NOTIFY_SCOPE_REQUIRED'));
      return;
    }

    this.roleService.updateNotifications(this.roleId, {
      notificationScope,
      notifyFromRoleIds
    }).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('ROLE.UPDATE_SUCCESS'));
        this.formSubmitted.emit();
      }
    });
  }
}
