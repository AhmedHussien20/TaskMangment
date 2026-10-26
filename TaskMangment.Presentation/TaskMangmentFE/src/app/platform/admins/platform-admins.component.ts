import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
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
  private fb = inject(FormBuilder);
  private api = inject(PlatformApiService);
  private modal = inject(NgbModal);
  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);

  rows: AdminRow[] = [];
  total = 0;
  page = 1;
  entries = 10;
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
      menuItems: (row: AdminRow) => [{
        label: row.isActive ? 'PLATFORM.SUSPEND' : 'PLATFORM.ACTIVATE',
        icon: row.isActive ? 'fe fe-pause-circle' : 'fe fe-play-circle',
        value: 'toggle',
        danger: row.isActive
      }]
    }
  ];
  form = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });
  formConfig: FormFieldConfig[] = [
    { type: 'input', inputType: 'text', label: 'PLATFORM.ADMIN_USER', name: 'fullName', validations: { required: true } },
    { type: 'input', inputType: 'email', label: 'PLATFORM.EMAIL', name: 'email', validations: { required: true } },
    { type: 'input', inputType: 'password', label: 'PLATFORM.PASSWORD', name: 'password', validations: { required: true, minlength: 6 } }
  ];

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

  openModal(content: unknown): void {
    this.form.reset();
    this.modal.open(content, { centered: true, backdrop: true, size: 'lg', windowClass: 'effect-scale' });
  }

  create(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    this.api.createAdmin({
      fullName: value.fullName || '',
      email: value.email || '',
      password: value.password || ''
    }).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('PLATFORM.CREATED_ADMIN'));
        this.modal.dismissAll();
        this.form.reset();
        this.load();
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.CREATE_ADMIN_FAILED'))
    });
  }

  onAction(event: { type: string; row: AdminRow }): void {
    this.api.setAdminActive(event.row.id, !event.row.isActive).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant(event.row.isActive ? 'PLATFORM.SUSPENDED_OK' : 'PLATFORM.ACTIVATED_OK'));
        this.load();
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.STATUS_FAILED'))
    });
  }
}
