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
  private traineeColumns: TableColumn[] = [
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
      menuItems: (row: TraineeRow) => [{
        label: row.isActive ? 'PLATFORM.SUSPEND' : 'PLATFORM.ACTIVATE',
        icon: row.isActive ? 'fe fe-pause-circle' : 'fe fe-play-circle',
        value: 'toggle',
        danger: row.isActive
      }]
    }
  ];
  form = this.fb.group({
    companyName: ['', Validators.required],
    adminFullName: ['', Validators.required],
    adminEmail: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });
  formConfig: FormFieldConfig[] = [
    { type: 'input', inputType: 'text', label: 'PLATFORM.COMPANY_NAME', name: 'companyName', validations: { required: true } },
    { type: 'input', inputType: 'text', label: 'PLATFORM.MANAGER_NAME', name: 'adminFullName', validations: { required: true } },
    { type: 'input', inputType: 'email', label: 'PLATFORM.MANAGER_EMAIL', name: 'adminEmail', validations: { required: true } },
    { type: 'input', inputType: 'password', label: 'PLATFORM.PASSWORD', name: 'password', validations: { required: true, minlength: 6 } }
  ];

  get columns(): TableColumn[] {
    if (this.session.isOrganizationAdmin) return this.traineeColumns;
    return [{ key: 'organizationName', label: 'PLATFORM.INSTITUTE' }, ...this.traineeColumns];
  }

  ngOnInit(): void {
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
    this.api.createTrainee({
      companyName: value.companyName || '',
      adminFullName: value.adminFullName || '',
      adminEmail: value.adminEmail || '',
      adminPassword: value.password || ''
    }).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('PLATFORM.CREATED_TRAINEE'));
        this.modal.dismissAll();
        this.form.reset();
        this.load();
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.CREATE_TRAINEE_FAILED'))
    });
  }

  onAction(event: { type: string; row: TraineeRow }): void {
    this.api.setTraineeActive(event.row.companyId, !event.row.isActive).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant(event.row.isActive ? 'PLATFORM.SUSPENDED_OK' : 'PLATFORM.ACTIVATED_OK'));
        this.load();
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.STATUS_FAILED'))
    });
  }
}
