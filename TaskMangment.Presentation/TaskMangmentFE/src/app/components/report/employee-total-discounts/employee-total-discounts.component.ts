import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';
import { EmployeeTotalDiscountReportRowDto, ExportType } from 'app/core/models/reports/reports';
import { Role } from 'app/core/models/roles/role';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { AuthService } from 'app/core/services/auth.service';
import { ReportListService } from 'app/core/services/report-list.service';
import { ReportPdfService } from 'app/core/services/report-pdf.service';
import { RoleService } from 'app/core/services/role.service';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';

@Component({
  selector: 'app-employee-total-discounts',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    TranslateModule,
    GenericTableComponent,
    PageHeaderComponent,
    DatePickerComponent
  ],
  templateUrl: './employee-total-discounts.component.html',
  styleUrls: ['./employee-total-discounts.component.scss']
})
export class EmployeeTotalDiscountsComponent implements OnInit {

  title = 'REPORTS.EMPLOYEE_TOTAL_DISCOUNTS';
  activeitem = 'REPORTS.EMPLOYEE_TOTAL_DISCOUNTS';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.EMPLOYEE_TOTAL_DISCOUNTS'];

  columns: TableColumn[] = [
    { key: 'employeeName', label: 'REPORTS.EMPLOYEE_NAME' },
    { key: 'roleTitle', label: 'REPORTS.ROLE_TITLE' },
    { key: 'totalDiscount', label: 'REPORTS.TOTAL_DISCOUNT' }
  ];

  rows: EmployeeTotalDiscountReportRowDto[] = [];
  roles: Role[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  selectedRoleId?: number;
  fromDate?: string;
  toDate?: string;

  isLoading = false;

  constructor(
    private reportService: ReportListService,
    private reportPdfService: ReportPdfService,
    private roleService: RoleService,
    private authService: AuthService,
    private router: Router,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    if ((this.authService.getRoleLevel() ?? 0) < 100) {
      this.router.navigate(['/report/reports-dashboard']);
      return;
    }

    this.loadRoles();
    this.loadData();
  }

  loadRoles(): void {
    const criteria: SearchCriteria = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 500,
      sortColumn: 'Name',
      sortDirection: 'ASC'
    };

    this.roleService.getAll(criteria).subscribe({
      next: (res: any) => {
        this.roles = res.data?.data ?? [];
      }
    });
  }

  loadData(): void {
    if (!this.validateDateRange()) return;

    this.isLoading = true;
    this.reportService
      .getEmployeeTotalDiscounts(this.selectedRoleId, this.selectedRoleTitle, this.fromDate, this.toDate)
      .subscribe({
        next: (res) => {
          this.rows = res.data ?? [];
          this.totalItems = this.rows.length;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
        }
      });
  }

  onFilterApply(): void {
    this.page = 1;
    this.loadData();
  }

  onPageChange(page: number): void {
    this.page = page;
  }

  onEntriesChange(entries: number): void {
    this.entries = entries;
    this.page = 1;
  }

  onExportPdf(): void {
    this.export(ExportType.Pdf, 'employee-total-discounts.pdf');
  }

  onExportExcel(): void {
    this.export(ExportType.Excel, 'employee-total-discounts.xlsx');
  }

  private export(exportType: ExportType, fileName: string): void {
    if (!this.validateDateRange()) return;

    this.reportPdfService
      .getEmployeeTotalDiscountsPdf(exportType, this.selectedRoleId, this.selectedRoleTitle, this.fromDate, this.toDate)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = fileName;
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
        },
        error: () => {
          this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
        }
      });
  }

  private validateDateRange(): boolean {
    if (this.toDate && !this.fromDate) {
      this.toastr.error(this.translate.instant('REPORTS.FROM_DATE_REQUIRED'));
      return false;
    }

    return true;
  }

  get selectedRoleTitle(): string | undefined {
    return this.roles.find(x => x.id === Number(this.selectedRoleId))?.name;
  }
}
