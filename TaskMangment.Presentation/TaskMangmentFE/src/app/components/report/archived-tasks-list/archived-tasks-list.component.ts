import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { ReportListService } from 'app/core/services/report-list.service';
import { ReportPdfService } from 'app/core/services/report-pdf.service';
import { EmployeeArchivedTasksReportDto, ExportType } from 'app/core/models/reports/reports';
import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';
import { ReportRoleFilterComponent } from '../shared/report-role-filter.component';
import { getReportRoleFilterParams } from '../shared/report-role-filter.helper';
import { ReportStatusFilterComponent } from '../shared/report-status-filter.component';

@Component({
  selector: 'app-archived-tasks-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    TranslateModule,
    GenericTableComponent,
    PageHeaderComponent,
    DatePickerComponent,
    ReportRoleFilterComponent,
    ReportStatusFilterComponent
  ],
  templateUrl: './archived-tasks-list.component.html',
  styleUrls: ['./archived-tasks-list.component.scss']
})
export class ArchivedTasksListComponent implements OnInit {

  @ViewChild(ReportRoleFilterComponent) roleFilter?: ReportRoleFilterComponent;

  title = 'REPORTS.EMPLOYEE_ARCHIVED_TASKS';
  activeitem = 'EMPLOYEE.LIST_TITLE';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.EMPLOYEE_ARCHIVED_TASKS'];
  exportType= ExportType.Pdf

  columns = [
    { key: 'employeeName', label: 'REPORTS.EMPLOYEE_NAME' },
    { key: 'archivedTasksCount', label: 'REPORTS.ARCHIVEED_TASKS_COUNT' },
    { key: 'totalTasks', label: 'REPORTS.TASKS_COUNT' },
    { key: 'archiveRate', label: 'REPORTS.ARCHIVE_RATE' },
  ];

  rows: EmployeeArchivedTasksReportDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  fromDate?: string;
  toDate?: string;
  selectedRoleId?: number;
  status?: string;

  isLoading = false;

  constructor(
    private reportService: ReportListService,
    private reportPdfService: ReportPdfService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;

    const { roleId, roleTitle } = getReportRoleFilterParams(this.roleFilter, this.selectedRoleId);
    this.reportService
      .getArchivedTasks(this.fromDate, this.toDate, roleId, roleTitle, this.status)
      .subscribe({
        next: (res) => {
          this.rows = res.data;
          this.totalItems = res.data.length;
          this.isLoading = false;
        },
        error: () => {
          this.isLoading = false;
          this.toastr.error(
            this.translate.instant('COMMON.ERROR_LOADING_DATA')
          );
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
    const { roleId, roleTitle } = getReportRoleFilterParams(this.roleFilter, this.selectedRoleId);
    this.reportPdfService
      .getArchivedTasksPdf(this.exportType, this.fromDate, this.toDate, roleId, roleTitle, this.status)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'archived-tasks-report.pdf';
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
        },
        error: () => {
          this.toastr.error(
            this.translate.instant('COMMON.ERROR_LOADING_DATA')
          );
        }
      });
  }
  onExportExcel(): void {
    const { roleId, roleTitle } = getReportRoleFilterParams(this.roleFilter, this.selectedRoleId);
    this.reportPdfService
      .getArchivedTasksPdf(ExportType.Excel, this.fromDate, this.toDate, roleId, roleTitle, this.status)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'archived-tasks-report.xlsx';
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
        },
        error: () => {
          this.toastr.error(
            this.translate.instant('COMMON.ERROR_LOADING_DATA')
          );
        }
      });
  }
}
