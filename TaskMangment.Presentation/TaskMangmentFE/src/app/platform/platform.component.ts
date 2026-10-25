import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, HostListener, OnDestroy, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { RouterModule } from '@angular/router';
import { NgbDropdownModule, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { Subscription } from 'rxjs';
import { TranslationService } from '../shared/services/translation.service';
import { AppStateService } from '../shared/services/app-state.service';
import { environment } from '../../environments/environment';
import { PageHeaderComponent } from '../shared/components/page-header/page-header.component';
import { GenericTableComponent, TableColumn } from '../shared/components/generic-table/generic-table.component';
import { GenericFormComponent } from '../shared/components/generic-form/generic-form.component';
import { SearchCriteria } from '../core/models/search-criteria.model';
import { FormFieldConfig } from '../core/models/form-field-config';

interface PlatformSession {
  userId: number;
  fullName: string;
  email: string;
  role: string;
  organizationId?: number;
  organizationName?: string;
  token: string;
}

interface OrganizationRow {
  id: number;
  name: string;
  isActive: boolean;
  createdDate: string;
  adminEmail?: string;
  adminFullName?: string;
}

interface TraineeRow {
  id: number;
  companyId: number;
  companyName: string;
  adminEmail: string;
  adminFullName: string;
  isActive: boolean;
  createdDate: string;
  actionLabel: string;
}

@Component({
  selector: 'app-platform',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    PageHeaderComponent,
    GenericTableComponent,
    GenericFormComponent,
    TranslateModule,
    NgbDropdownModule
  ],
  templateUrl: './platform.component.html',
  styleUrls: ['./platform.component.scss']
})
export class PlatformComponent implements OnInit, OnDestroy {
  session: PlatformSession | null = null;
  error = '';
  loading = false;
  showLoginPassword = false;

  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private modalService = inject(NgbModal);
  private toastr = inject(ToastrService);
  private translation = inject(TranslationService);
  private translate = inject(TranslateService);
  private appState = inject(AppStateService);
  private cdr = inject(ChangeDetectorRef);
  private langSub?: Subscription;

  countryFlag = 'sa';
  isFullscreen = false;

  private allOrganizations: OrganizationRow[] = [];
  private allTrainees: TraineeRow[] = [];

  orgRows: OrganizationRow[] = [];
  traineeRows: TraineeRow[] = [];
  orgTotal = 0;
  traineeTotal = 0;
  orgPage = 1;
  traineePage = 1;
  orgEntries = 10;
  traineeEntries = 10;

  statusOptions = [
    { id: 'active', name: 'PLATFORM.ACTIVE' },
    { id: 'inactive', name: 'PLATFORM.SUSPENDED' }
  ];

  orgColumns: TableColumn[] = [
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

  traineeColumns: TableColumn[] = [
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
    { key: 'actionLabel', label: 'PLATFORM.ACTION', type: 'icon-action', icon: 'fe fe-power' }
  ];

  orgCriteria: SearchCriteria = new SearchCriteria({
    searchKey: '',
    status: null,
    pageIndex: 1,
    pageSize: 10,
    sortColumn: 'name',
    sortDirection: 'ASC',
    filterTypes: { searchKey: 'text', status: 'dropdown' }
  });

  traineeCriteria: SearchCriteria = new SearchCriteria({
    searchKey: '',
    status: null,
    pageIndex: 1,
    pageSize: 10,
    sortColumn: 'companyName',
    sortDirection: 'ASC',
    filterTypes: { searchKey: 'text', status: 'dropdown' }
  });

  orgLabels = { searchKey: 'PLATFORM.SEARCH', status: 'PLATFORM.STATUS' };
  traineeLabels = { searchKey: 'PLATFORM.SEARCH', status: 'PLATFORM.STATUS' };

  organizationFormConfig: FormFieldConfig[] = [
    { type: 'input', inputType: 'text', label: 'PLATFORM.INSTITUTE_NAME', name: 'name', validations: { required: true } },
    { type: 'input', inputType: 'text', label: 'PLATFORM.ADMIN_NAME', name: 'adminFullName', validations: { required: true } },
    { type: 'input', inputType: 'email', label: 'PLATFORM.ADMIN_EMAIL', name: 'adminEmail', validations: { required: true } },
    { type: 'input', inputType: 'password', label: 'PLATFORM.PASSWORD', name: 'password', validations: { required: true, minlength: 6 } }
  ];

  traineeFormConfig: FormFieldConfig[] = [
    { type: 'input', inputType: 'text', label: 'PLATFORM.COMPANY_NAME', name: 'companyName', validations: { required: true } },
    { type: 'input', inputType: 'text', label: 'PLATFORM.MANAGER_NAME', name: 'adminFullName', validations: { required: true } },
    { type: 'input', inputType: 'email', label: 'PLATFORM.MANAGER_EMAIL', name: 'adminEmail', validations: { required: true } },
    { type: 'input', inputType: 'password', label: 'PLATFORM.PASSWORD', name: 'password', validations: { required: true, minlength: 6 } }
  ];

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  organizationForm = this.fb.group({
    name: ['', Validators.required],
    adminFullName: ['', Validators.required],
    adminEmail: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  traineeForm = this.fb.group({
    companyName: ['', Validators.required],
    adminFullName: ['', Validators.required],
    adminEmail: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  ngOnInit(): void {
    this.langSub = this.translation.getCurrentLang().subscribe(lang => {
      this.countryFlag = lang === 'ar' ? 'sa' : 'gb';
      this.refreshActionLabels();
      this.sliceTrainees();
    });
    const raw = localStorage.getItem('platformUser');
    const token = localStorage.getItem('platformToken');
    if (raw && token) {
      this.session = JSON.parse(raw);
      this.load();
    }
    this.syncBodyClass();
  }

  ngOnDestroy(): void {
    this.langSub?.unsubscribe();
    document.body.classList.remove('error-1');
  }

  changeLanguage(lang: string): void {
    this.translation.setLanguage(lang);
  }

  updateTheme(theme: string): void {
    this.appState.updateState({ theme, menuColor: theme });
    if (theme === 'light') {
      this.appState.updateState({ theme, themeBackground: '', headerColor: 'light', menuColor: 'dark' });
    }
    if (theme === 'dark') {
      this.appState.updateState({ theme, themeBackground: '', headerColor: 'dark', menuColor: 'dark' });
    }
    const html = document.querySelector('html');
    html?.style.removeProperty('--body-bg-rgb');
    html?.style.removeProperty('--body-bg-rgb2');
    html?.style.removeProperty('--light-rgb');
    html?.style.removeProperty('--form-control-bg');
    html?.style.removeProperty('--input-border');
    html?.style.removeProperty('--sidemenu-active-bgcolor');
  }

  toggleFullscreen(): void {
    if (this.isFullscreen) {
      document.exitFullscreen?.();
    } else {
      document.documentElement.requestFullscreen?.();
    }
  }

  @HostListener('document:fullscreenchange')
  onFullscreenChange(): void {
    this.isFullscreen = !!document.fullscreenElement;
    this.cdr.detectChanges();
  }

  get isPlatformAdmin(): boolean {
    return this.session?.role === 'PlatformAdmin';
  }

  login(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.error = '';
    this.loading = true;
    this.http.post<any>(`${environment.apiUrl}/platform/login`, this.loginForm.value).subscribe({
      next: (res) => {
        this.loading = false;
        const data = res.data as PlatformSession;
        this.session = data;
        localStorage.setItem('platformToken', data.token);
        localStorage.setItem('platformUser', JSON.stringify(data));
        this.syncBodyClass();
        this.load();
      },
      error: (err) => {
        this.loading = false;
        this.error = err?.error?.message || this.translate.instant('PLATFORM.LOGIN_FAILED');
      }
    });
  }

  logout(): void {
    localStorage.removeItem('platformToken');
    localStorage.removeItem('platformUser');
    this.session = null;
    this.allOrganizations = [];
    this.allTrainees = [];
    this.orgRows = [];
    this.traineeRows = [];
    this.error = '';
    this.modalService.dismissAll();
    this.syncBodyClass();
  }

  openOrganizationModal(content: unknown): void {
    this.organizationForm.reset();
    this.modalService.open(content, {
      centered: true,
      backdrop: true,
      size: 'lg',
      windowClass: 'effect-scale'
    });
  }

  openTraineeModal(content: unknown): void {
    this.traineeForm.reset();
    this.modalService.open(content, {
      centered: true,
      backdrop: true,
      size: 'lg',
      windowClass: 'effect-scale'
    });
  }

  createOrganization(): void {
    if (this.organizationForm.invalid) {
      this.organizationForm.markAllAsTouched();
      return;
    }

    const value = this.organizationForm.getRawValue();
    this.http.post(`${environment.apiUrl}/platform/organizations`, {
      name: value.name,
      adminFullName: value.adminFullName,
      adminEmail: value.adminEmail,
      adminPassword: value.password
    }).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('PLATFORM.CREATED_INSTITUTE'));
        this.modalService.dismissAll();
        this.organizationForm.reset();
        this.load();
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.CREATE_INSTITUTE_FAILED'))
    });
  }

  createTrainee(): void {
    if (this.traineeForm.invalid) {
      this.traineeForm.markAllAsTouched();
      return;
    }

    const value = this.traineeForm.getRawValue();
    this.http.post(`${environment.apiUrl}/platform/trainees`, {
      companyName: value.companyName,
      adminFullName: value.adminFullName,
      adminEmail: value.adminEmail,
      adminPassword: value.password
    }).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('PLATFORM.CREATED_TRAINEE'));
        this.modalService.dismissAll();
        this.traineeForm.reset();
        this.load();
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.CREATE_TRAINEE_FAILED'))
    });
  }

  onTraineeAction(event: { type: string; row: TraineeRow }): void {
    this.http.post(`${environment.apiUrl}/platform/trainees/${event.row.companyId}/active`, {
      isActive: !event.row.isActive
    }).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant(event.row.isActive ? 'PLATFORM.SUSPENDED_OK' : 'PLATFORM.ACTIVATED_OK'));
        this.load();
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.STATUS_FAILED'))
    });
  }

  applyOrganizationFilters(filters: SearchCriteria): void {
    this.orgCriteria = { ...this.orgCriteria, ...filters, pageIndex: 1 };
    this.orgPage = 1;
    this.sliceOrganizations();
  }

  applyTraineeFilters(filters: SearchCriteria): void {
    this.traineeCriteria = { ...this.traineeCriteria, ...filters, pageIndex: 1 };
    this.traineePage = 1;
    this.sliceTrainees();
  }

  onOrgPageChange(page: number): void {
    this.orgPage = page;
    this.orgCriteria.pageIndex = page;
    this.sliceOrganizations();
  }

  onTraineePageChange(page: number): void {
    this.traineePage = page;
    this.traineeCriteria.pageIndex = page;
    this.sliceTrainees();
  }

  onOrgEntriesChange(entries: number): void {
    this.orgEntries = entries;
    this.orgPage = 1;
    this.orgCriteria.pageSize = entries;
    this.orgCriteria.pageIndex = 1;
    this.sliceOrganizations();
  }

  onTraineeEntriesChange(entries: number): void {
    this.traineeEntries = entries;
    this.traineePage = 1;
    this.traineeCriteria.pageSize = entries;
    this.traineeCriteria.pageIndex = 1;
    this.sliceTrainees();
  }

  private syncBodyClass(): void {
    document.body.classList.toggle('error-1', !this.session);
  }

  private load(): void {
    if (this.isPlatformAdmin) {
      this.http.get<any>(`${environment.apiUrl}/platform/organizations`).subscribe({
        next: (res) => {
          this.allOrganizations = res.data || [];
          this.sliceOrganizations();
        },
        error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.LOAD_INSTITUTES_FAILED'))
      });
      return;
    }

    this.http.get<any>(`${environment.apiUrl}/platform/trainees`).subscribe({
      next: (res) => {
        this.allTrainees = (res.data || []).map((row: TraineeRow) => ({
          ...row,
          id: row.companyId,
          actionLabel: ''
        }));
        this.refreshActionLabels();
        this.sliceTrainees();
      },
      error: (err) => this.toastr.error(err?.error?.message || this.translate.instant('PLATFORM.LOAD_TRAINEES_FAILED'))
    });
  }

  private refreshActionLabels(): void {
    this.allTrainees = this.allTrainees.map(row => ({
      ...row,
      actionLabel: this.translate.instant(row.isActive ? 'PLATFORM.SUSPEND' : 'PLATFORM.ACTIVATE')
    }));
  }

  private sliceOrganizations(): void {
    const filtered = this.filterRows(this.allOrganizations, this.orgCriteria, row =>
      `${row.name} ${row.adminFullName || ''} ${row.adminEmail || ''}`
    );
    this.orgTotal = filtered.length;
    const start = (this.orgPage - 1) * this.orgEntries;
    this.orgRows = filtered.slice(start, start + this.orgEntries);
  }

  private sliceTrainees(): void {
    const filtered = this.filterRows(this.allTrainees, this.traineeCriteria, row =>
      `${row.companyName} ${row.adminFullName} ${row.adminEmail}`
    );
    this.traineeTotal = filtered.length;
    const start = (this.traineePage - 1) * this.traineeEntries;
    this.traineeRows = filtered.slice(start, start + this.traineeEntries);
  }

  private filterRows<T extends { isActive: boolean }>(
    source: T[],
    criteria: SearchCriteria,
    textOf: (row: T) => string
  ): T[] {
    const term = String(criteria.searchKey || '').trim().toLowerCase();
    const status = criteria['status'];
    const filtered = source.filter(row => {
      if (term && !textOf(row).toLowerCase().includes(term)) {
        return false;
      }
      if (status === 'active' && !row.isActive) {
        return false;
      }
      if (status === 'inactive' && row.isActive) {
        return false;
      }
      return true;
    });

    const column = String(criteria.sortColumn || '');
    const direction = criteria.sortDirection === 'DESC' ? -1 : 1;
    return filtered.sort((a, b) => {
      const av = (a as any)[column];
      const bv = (b as any)[column];
      if (av == null && bv == null) return 0;
      if (typeof av === 'string' || typeof bv === 'string') {
        return String(av ?? '').localeCompare(String(bv ?? ''), 'ar') * direction;
      }
      if (av > bv) return direction;
      if (av < bv) return -direction;
      return 0;
    });
  }
}
