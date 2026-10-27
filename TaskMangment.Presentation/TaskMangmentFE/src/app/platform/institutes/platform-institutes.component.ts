import { CommonModule } from '@angular/common';
import { Component, OnInit, TemplateRef, ViewChild, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';
import { FormFieldConfig } from '../../core/models/form-field-config';
import { SearchCriteria } from '../../core/models/search-criteria.model';
import { GenericFormComponent } from '../../shared/components/generic-form/generic-form.component';
import { GenericTableComponent, TableColumn } from '../../shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { PlatformApiService } from '../platform-api.service';
import { PlatformSessionService } from '../platform-session.service';

interface OrganizationRow {
  id: number;
  name: string;
  isActive: boolean;
  createdDate: string;
  adminId?: number;
  adminEmail?: string;
  adminFullName?: string;
}

@Component({
  selector: 'app-platform-institutes',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent, GenericTableComponent, GenericFormComponent],
  templateUrl: './platform-institutes.component.html'
})
export class PlatformInstitutesComponent implements OnInit {
  @ViewChild('organizationModal') organizationModal!: TemplateRef<unknown>;

  session = inject(PlatformSessionService);
  private fb = inject(FormBuilder);
  private api = inject(PlatformApiService);
  private modal = inject(NgbModal);
  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);

  rows: OrganizationRow[] = [];
  total = 0;
  page = 1;
  entries = 10;
  editingId: number | null = null;
  statusOptions = [
    { id: 'active', name: 'PLATFORM.ACTIVE' },
    { id: 'inactive', name: 'PLATFORM.SUSPENDED' }
  ];
  labels = { searchKey: 'PLATFORM.SEARCH', status: 'PLATFORM.STATUS' };
  criteria = new SearchCriteria({
    searchKey: '',
    status: null,
    pageIndex: 1,
    pageSize: 10,
    sortColumn: 'name',
    sortDirection: 'ASC',
    filterTypes: { searchKey: 'text', status: 'dropdown' }
  });
  private baseColumns: TableColumn[] = [
    { key: 'name', label: 'PLATFORM.INSTITUTE' },
    { key: 'adminFullName', label: 'PLATFORM.ADMIN' },
    { key: 'adminEmail', label: 'PLATFORM.EMAIL_COL' },
    { key: 'createdDate', label: 'PLATFORM.CREATED', type: 'date' },
    {
      key: 'isActive',
      label: 'PLATFORM.STATUS',
      type: 'badge',
      badgeMap: {
        true: { text: 'PLATFORM.ACTIVE', class: 'bg-success' },
        false: { text: 'PLATFORM.SUSPENDED', class: 'bg-danger' }
      }
    }
  ];

  get columns(): TableColumn[] {
    if (!this.canManage) return this.baseColumns;
    return [
      ...this.baseColumns,
      {
        key: 'actions',
        label: 'PLATFORM.ACTION',
        type: 'menu',
        width: '72px',
        menuItems: () => [
          { label: 'PLATFORM.EDIT', icon: 'fe fe-edit', value: 'edit' },
          { label: 'PLATFORM.DELETE', icon: 'fe fe-trash-2', value: 'delete', danger: true }
        ]
      }
    ];
  }
  form = this.fb.group({
    name: ['', Validators.required],
    adminFullName: ['', Validators.required],
    adminEmail: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required]
  }, { validators: this.passwordsMatch });
  formConfig: FormFieldConfig[] = [
    { type: 'input', inputType: 'text', label: 'PLATFORM.INSTITUTE_NAME', name: 'name', validations: { required: true } },
    { type: 'input', inputType: 'text', label: 'PLATFORM.ADMIN_NAME', name: 'adminFullName', validations: { required: true } },
    { type: 'input', inputType: 'email', label: 'PLATFORM.ADMIN_EMAIL', name: 'adminEmail', validations: { required: true } },
    { type: 'input', inputType: 'password', label: 'PLATFORM.PASSWORD', name: 'password', validations: { required: true, minlength: 6 } },
    { type: 'input', inputType: 'password', label: 'PLATFORM.CONFIRM_PASSWORD', name: 'confirmPassword', validations: { required: true } }
  ];

  get canManage(): boolean {
    return this.session.isSuperAdmin || this.session.isPlatformAdmin;
  }

  get modalTitle(): string {
    return this.editingId ? 'PLATFORM.EDIT_INSTITUTE' : 'PLATFORM.NEW_INSTITUTE';
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.organizations(this.criteria).subscribe({
      next: (res) => {
        const page = res.data || {};
        this.rows = page.data || [];
        this.total = page.totalCount || 0;
        this.page = page.pageIndex || this.criteria.pageIndex;
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.LOAD_INSTITUTES_FAILED'))
    });
  }

  applyFilters(filters: SearchCriteria): void {
    this.criteria = { ...this.criteria, ...filters, pageIndex: 1 };
    this.page = 1;
    this.load();
  }

  onPageChange(page: number): void {
    this.page = page;
    this.criteria.pageIndex = page;
    this.load();
  }

  onEntriesChange(entries: number): void {
    this.entries = entries;
    this.page = 1;
    this.criteria.pageSize = entries;
    this.criteria.pageIndex = 1;
    this.load();
  }

  openCreate(content: unknown): void {
    this.editingId = null;
    this.applyPasswordValidators(true);
    this.form.reset();
    this.modal.open(content, { centered: true, backdrop: true, size: 'lg', windowClass: 'effect-scale' });
  }

  openEdit(row: OrganizationRow): void {
    this.editingId = row.id;
    this.applyPasswordValidators(false);
    this.form.reset({
      name: row.name,
      adminFullName: row.adminFullName || '',
      adminEmail: row.adminEmail || '',
      password: '',
      confirmPassword: ''
    });
    this.modal.open(this.organizationModal, { centered: true, backdrop: true, size: 'lg', windowClass: 'effect-scale' });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const body = {
      name: value.name || '',
      adminFullName: value.adminFullName || '',
      adminEmail: value.adminEmail || '',
      adminPassword: value.password || ''
    };

    const request$ = this.editingId
      ? this.api.updateOrganization(this.editingId, {
          name: body.name,
          adminFullName: body.adminFullName,
          adminEmail: body.adminEmail,
          adminPassword: body.adminPassword || undefined
        })
      : this.api.createOrganization(body);

    request$.subscribe({
      next: () => {
        this.toastr.success(this.translate.instant(this.editingId ? 'PLATFORM.UPDATED_INSTITUTE' : 'PLATFORM.CREATED_INSTITUTE'));
        this.modal.dismissAll();
        this.form.reset();
        this.editingId = null;
        this.load();
      },
      error: (err) => this.toastr.error(
        err?.error?.message ||
        this.translate.instant(this.editingId ? 'PLATFORM.UPDATE_INSTITUTE_FAILED' : 'PLATFORM.CREATE_INSTITUTE_FAILED')
      )
    });
  }

  onAction(event: { type: string; row: OrganizationRow }): void {
    if (event.type === 'edit') {
      this.openEdit(event.row);
      return;
    }
    if (event.type === 'delete') {
      this.confirmDelete(event.row);
    }
  }

  private confirmDelete(row: OrganizationRow): void {
    Swal.fire({
      title: this.translate.instant('COMMON.CONFIRM_DELETE_TITLE'),
      text: this.translate.instant('PLATFORM.DELETE_INSTITUTE_CONFIRM'),
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: this.translate.instant('COMMON.DELETE_BUTTON'),
      cancelButtonText: this.translate.instant('COMMON.CANCEL_BUTTON'),
      confirmButtonColor: '#d33',
      cancelButtonColor: '#6c757d'
    }).then((result) => {
      if (!result.isConfirmed) return;
      this.api.deleteOrganization(row.id).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('PLATFORM.DELETED_INSTITUTE'));
          this.load();
        },
        error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.DELETE_INSTITUTE_FAILED'))
      });
    });
  }

  private applyPasswordValidators(required: boolean): void {
    const password = this.form.get('password');
    const confirm = this.form.get('confirmPassword');
    if (required) {
      password?.setValidators([Validators.required, Validators.minLength(6)]);
      confirm?.setValidators([Validators.required]);
      this.formConfig = this.formConfig.map((f) =>
        f.name === 'password' || f.name === 'confirmPassword'
          ? { ...f, validations: { ...(f.validations || {}), required: true, ...(f.name === 'password' ? { minlength: 6 } : {}) } }
          : f
      );
    } else {
      password?.setValidators([Validators.minLength(6)]);
      confirm?.clearValidators();
      this.formConfig = this.formConfig.map((f) =>
        f.name === 'password' || f.name === 'confirmPassword'
          ? { ...f, validations: f.name === 'password' ? { minlength: 6 } : {} }
          : f
      );
    }
    password?.updateValueAndValidity({ emitEvent: false });
    confirm?.updateValueAndValidity({ emitEvent: false });
  }

  private passwordsMatch(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password');
    const confirm = group.get('confirmPassword');
    if (!password || !confirm) return null;

    const passwordValue = password.value || '';
    const confirmValue = confirm.value || '';

    if (!passwordValue && !confirmValue) {
      if (confirm.errors?.['passwordMismatch']) {
        const { passwordMismatch, ...rest } = confirm.errors;
        confirm.setErrors(Object.keys(rest).length ? rest : null);
      }
      return null;
    }

    if (passwordValue !== confirmValue) {
      confirm.setErrors({ ...(confirm.errors || {}), passwordMismatch: true });
      return { passwordMismatch: true };
    }

    if (confirm.errors) {
      const { passwordMismatch, ...rest } = confirm.errors;
      confirm.setErrors(Object.keys(rest).length ? rest : null);
    }
    return null;
  }
}
