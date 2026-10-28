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

interface TraineeRow {
  id: number;
  companyId: number;
  companyName: string;
  adminEmail: string;
  adminFullName: string;
  isActive: boolean;
  createdDate: string;
  organizationName?: string;
}

@Component({
  selector: 'app-platform-trainees',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, PageHeaderComponent, GenericTableComponent, GenericFormComponent],
  templateUrl: './platform-trainees.component.html'
})
export class PlatformTraineesComponent implements OnInit {
  @ViewChild('traineeModal') traineeModal!: TemplateRef<unknown>;

  session = inject(PlatformSessionService);
  private fb = inject(FormBuilder);
  private api = inject(PlatformApiService);
  private modal = inject(NgbModal);
  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);

  rows: TraineeRow[] = [];
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
    sortColumn: 'companyName',
    sortDirection: 'ASC',
    filterTypes: { searchKey: 'text', status: 'dropdown' }
  });
  // Stable reference — avoid recreating the columns array every CD cycle (breaks ngbDropdown).
  columns: TableColumn[] = [];
  form = this.fb.group({
    organizationId: [null as number | null],
    companyName: ['', Validators.required],
    adminFullName: ['', Validators.required],
    adminEmail: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    confirmPassword: ['', Validators.required]
  }, { validators: this.passwordsMatch });
  formConfig: FormFieldConfig[] = [];

  get canCreate(): boolean {
    return this.session.isOrganizationAdmin || this.session.isPlatformAdmin || this.session.isSuperAdmin;
  }

  get needsOrganizationPicker(): boolean {
    return this.session.isPlatformAdmin || this.session.isSuperAdmin;
  }

  get modalTitle(): string {
    return this.editingId ? 'PLATFORM.EDIT_TRAINEE' : 'PLATFORM.NEW_TRAINEE';
  }

  ngOnInit(): void {
    this.columns = [
      ...(this.session.isOrganizationAdmin
        ? []
        : [{ key: 'organizationName', label: 'PLATFORM.INSTITUTE' } as TableColumn]),
      { key: 'companyName', label: 'PLATFORM.COMPANY' },
      { key: 'adminFullName', label: 'PLATFORM.MANAGER' },
      { key: 'adminEmail', label: 'PLATFORM.EMAIL_COL' },
      { key: 'createdDate', label: 'PLATFORM.CREATED', type: 'date' },
      {
        key: 'isActive',
        label: 'PLATFORM.STATUS',
        type: 'badge',
        badgeMap: {
          true: { text: 'PLATFORM.ACTIVE_F', class: 'bg-success' },
          false: { text: 'PLATFORM.SUSPENDED_F', class: 'bg-danger' }
        }
      },
      {
        key: 'actions',
        label: 'PLATFORM.ACTION',
        type: 'menu',
        width: '72px',
        menuItems: (row: TraineeRow) => [
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
    this.rebuildFormConfig();
    this.load();
  }

  load(): void {
    this.api.trainees(this.criteria).subscribe({
      next: (res) => {
        const page = res.data || {};
        this.rows = (page.data || []).map((row: TraineeRow) => ({ ...row, id: row.companyId }));
        this.total = page.totalCount || 0;
        this.page = page.pageIndex || this.criteria.pageIndex;
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.LOAD_TRAINEES_FAILED'))
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
    this.rebuildFormConfig(true);
    this.form.reset();
    if (this.needsOrganizationPicker) {
      this.form.get('organizationId')?.setValidators([Validators.required]);
    } else {
      this.form.get('organizationId')?.clearValidators();
    }
    this.form.get('organizationId')?.updateValueAndValidity({ emitEvent: false });
    this.modal.open(content, { centered: true, backdrop: true, size: 'lg', windowClass: 'effect-scale' });
  }

  openEdit(row: TraineeRow): void {
    this.editingId = row.companyId;
    this.applyPasswordValidators(false);
    this.rebuildFormConfig(false);
    this.form.get('organizationId')?.clearValidators();
    this.form.get('organizationId')?.updateValueAndValidity({ emitEvent: false });
    this.form.reset({
      organizationId: null,
      companyName: row.companyName,
      adminFullName: row.adminFullName,
      adminEmail: row.adminEmail,
      password: '',
      confirmPassword: ''
    });
    this.modal.open(this.traineeModal, { centered: true, backdrop: true, size: 'lg', windowClass: 'effect-scale' });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    const request$ = this.editingId
      ? this.api.updateTrainee(this.editingId, {
          companyName: value.companyName || '',
          adminFullName: value.adminFullName || '',
          adminEmail: value.adminEmail || '',
          adminPassword: value.password || undefined
        })
      : this.api.createTrainee({
          companyName: value.companyName || '',
          adminFullName: value.adminFullName || '',
          adminEmail: value.adminEmail || '',
          adminPassword: value.password || '',
          organizationId: this.needsOrganizationPicker ? (value.organizationId || undefined) : undefined
        });

    request$.subscribe({
      next: () => {
        this.toastr.success(this.translate.instant(this.editingId ? 'PLATFORM.UPDATED_TRAINEE' : 'PLATFORM.CREATED_TRAINEE'));
        this.modal.dismissAll();
        this.form.reset();
        this.editingId = null;
        this.load();
      },
      error: (err) => this.toastr.error(
        err?.error?.message ||
        this.translate.instant(this.editingId ? 'PLATFORM.UPDATE_TRAINEE_FAILED' : 'PLATFORM.CREATE_TRAINEE_FAILED')
      )
    });
  }

  onAction(event: { type: string; row: TraineeRow }): void {
    if (event.type === 'edit') {
      this.openEdit(event.row);
      return;
    }
    if (event.type === 'delete') {
      this.confirmDelete(event.row);
      return;
    }
    this.api.setTraineeActive(event.row.companyId, !event.row.isActive).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant(event.row.isActive ? 'PLATFORM.SUSPENDED_OK' : 'PLATFORM.ACTIVATED_OK'));
        this.load();
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.STATUS_FAILED'))
    });
  }

  private confirmDelete(row: TraineeRow): void {
    Swal.fire({
      title: this.translate.instant('COMMON.CONFIRM_DELETE_TITLE'),
      text: this.translate.instant('PLATFORM.DELETE_TRAINEE_CONFIRM'),
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: this.translate.instant('COMMON.DELETE_BUTTON'),
      cancelButtonText: this.translate.instant('COMMON.CANCEL_BUTTON'),
      confirmButtonColor: '#d33',
      cancelButtonColor: '#6c757d'
    }).then((result) => {
      if (!result.isConfirmed) return;
      this.api.deleteTrainee(row.companyId).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('PLATFORM.DELETED_TRAINEE'));
          this.load();
        },
        error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.DELETE_TRAINEE_FAILED'))
      });
    });
  }

  private rebuildFormConfig(includeOrganization = !this.editingId && this.needsOrganizationPicker): void {
    const fields: FormFieldConfig[] = [];
    if (includeOrganization && this.needsOrganizationPicker) {
      fields.push({
        type: 'select',
        label: 'PLATFORM.INSTITUTE',
        name: 'organizationId',
        selectType: 'simple',
        options: [],
        validations: { required: true },
        isPaginated: true,
        // Loaded only when the dropdown opens (generic-form onSelectOpen / search / scroll).
        searchFunction: (searchTerm: string, page: number) =>
          this.api.organizations(new SearchCriteria({
            searchKey: searchTerm || '',
            pageIndex: page || 1,
            pageSize: 20,
            sortColumn: 'name',
            sortDirection: 'ASC',
            status: 'active',
            filterTypes: { searchKey: 'text', status: 'dropdown' }
          }))
      });
    }
    fields.push(
      { type: 'input', inputType: 'text', label: 'PLATFORM.COMPANY_NAME', name: 'companyName', validations: { required: true } },
      { type: 'input', inputType: 'text', label: 'PLATFORM.MANAGER_NAME', name: 'adminFullName', validations: { required: true } },
      { type: 'input', inputType: 'email', label: 'PLATFORM.MANAGER_EMAIL', name: 'adminEmail', validations: { required: true } },
      { type: 'input', inputType: 'password', label: 'PLATFORM.PASSWORD', name: 'password', validations: { required: !this.editingId, minlength: 6 } },
      { type: 'input', inputType: 'password', label: 'PLATFORM.CONFIRM_PASSWORD', name: 'confirmPassword', validations: { required: !this.editingId } }
    );
    this.formConfig = fields;
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
