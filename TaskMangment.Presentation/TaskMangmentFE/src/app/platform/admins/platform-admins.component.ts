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

interface AdminRow {
  id: number;
  fullName: string;
  email: string;
  isActive: boolean;
  createdDate: string;
}

@Component({
  selector: 'app-platform-admins',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent, GenericTableComponent, GenericFormComponent],
  templateUrl: './platform-admins.component.html'
})
export class PlatformAdminsComponent implements OnInit {
  @ViewChild('adminModal') adminModal!: TemplateRef<unknown>;

  private fb = inject(FormBuilder);
  private api = inject(PlatformApiService);
  private modal = inject(NgbModal);
  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);

  rows: AdminRow[] = [];
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
    sortColumn: 'fullName',
    sortDirection: 'ASC',
    filterTypes: { searchKey: 'text', status: 'dropdown' }
  });
  columns: TableColumn[] = [
    { key: 'fullName', label: 'PLATFORM.ADMIN_USER' },
    { key: 'email', label: 'PLATFORM.EMAIL_COL' },
    { key: 'createdDate', label: 'PLATFORM.CREATED', type: 'date' },
    {
      key: 'isActive',
      label: 'PLATFORM.STATUS',
      type: 'badge',
      badgeMap: {
        true: { text: 'PLATFORM.ACTIVE', class: 'bg-success' },
        false: { text: 'PLATFORM.SUSPENDED', class: 'bg-danger' }
      }
    },
    {
      key: 'actions',
      label: 'PLATFORM.ACTION',
      type: 'menu',
      width: '72px',
      menuItems: (row: AdminRow) => [
        { label: 'PLATFORM.EDIT', icon: 'fe fe-edit', value: 'edit' },
        {
          label: row.isActive ? 'PLATFORM.SUSPEND' : 'PLATFORM.ACTIVATE',
          icon: row.isActive ? 'fe fe-pause-circle' : 'fe fe-play-circle',
          value: 'toggle',
          danger: row.isActive
        },
        { label: 'PLATFORM.DELETE', icon: 'fe fe-trash-2', value: 'delete', danger: true }
      ]
    }
  ];
  form = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required]
  }, { validators: this.passwordsMatch });
  formConfig: FormFieldConfig[] = [
    { type: 'input', inputType: 'text', label: 'PLATFORM.ADMIN_USER', name: 'fullName', validations: { required: true } },
    { type: 'input', inputType: 'email', label: 'PLATFORM.EMAIL', name: 'email', validations: { required: true } },
    { type: 'input', inputType: 'password', label: 'PLATFORM.PASSWORD', name: 'password', validations: { required: true, minlength: 6 } },
    { type: 'input', inputType: 'password', label: 'PLATFORM.CONFIRM_PASSWORD', name: 'confirmPassword', validations: { required: true } }
  ];

  get modalTitle(): string {
    return this.editingId ? 'PLATFORM.EDIT_ADMIN' : 'PLATFORM.NEW_ADMIN';
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.api.admins(this.criteria).subscribe({
      next: (res) => {
        const page = res.data || {};
        this.rows = page.data || [];
        this.total = page.totalCount || 0;
        this.page = page.pageIndex || this.criteria.pageIndex;
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.LOAD_ADMINS_FAILED'))
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

  openEdit(row: AdminRow): void {
    this.editingId = row.id;
    this.applyPasswordValidators(false);
    this.form.reset({
      fullName: row.fullName,
      email: row.email,
      password: '',
      confirmPassword: ''
    });
    this.modal.open(this.adminModal, { centered: true, backdrop: true, size: 'lg', windowClass: 'effect-scale' });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const request$ = this.editingId
      ? this.api.updateAdmin(this.editingId, {
          fullName: value.fullName || '',
          email: value.email || '',
          password: value.password || undefined
        })
      : this.api.createAdmin({
          fullName: value.fullName || '',
          email: value.email || '',
          password: value.password || ''
        });

    request$.subscribe({
      next: () => {
        this.toastr.success(this.translate.instant(this.editingId ? 'PLATFORM.UPDATED_ADMIN' : 'PLATFORM.CREATED_ADMIN'));
        this.modal.dismissAll();
        this.form.reset();
        this.editingId = null;
        this.load();
      },
      error: (err) => this.toastr.error(
        err?.error?.message ||
        this.translate.instant(this.editingId ? 'PLATFORM.UPDATE_ADMIN_FAILED' : 'PLATFORM.CREATE_ADMIN_FAILED')
      )
    });
  }

  onAction(event: { type: string; row: AdminRow }): void {
    if (event.type === 'edit') {
      this.openEdit(event.row);
      return;
    }
    if (event.type === 'delete') {
      this.confirmDelete(event.row);
      return;
    }
    this.api.setAdminActive(event.row.id, !event.row.isActive).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant(event.row.isActive ? 'PLATFORM.SUSPENDED_OK' : 'PLATFORM.ACTIVATED_OK'));
        this.load();
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.STATUS_FAILED'))
    });
  }

  private confirmDelete(row: AdminRow): void {
    Swal.fire({
      title: this.translate.instant('COMMON.CONFIRM_DELETE_TITLE'),
      text: this.translate.instant('PLATFORM.DELETE_ADMIN_CONFIRM'),
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: this.translate.instant('COMMON.DELETE_BUTTON'),
      cancelButtonText: this.translate.instant('COMMON.CANCEL_BUTTON'),
      confirmButtonColor: '#d33',
      cancelButtonColor: '#6c757d'
    }).then((result) => {
      if (!result.isConfirmed) return;
      this.api.deleteAdmin(row.id).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('PLATFORM.DELETED_ADMIN'));
          this.load();
        },
        error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.DELETE_ADMIN_FAILED'))
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
