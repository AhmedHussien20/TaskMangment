import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericTableComponent, TableColumn } from '../../../shared/components/generic-table/generic-table.component';
import { ReportListService } from 'app/core/services/report-list.service';
import { EmployeeCommentsReportDto, ExportType } from 'app/core/models/reports/reports';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { ReportPdfService } from 'app/core/services/report-pdf.service';
import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';
import { ReportRoleFilterComponent } from '../shared/report-role-filter/report-role-filter.component';
import { getReportRoleFilterParams } from '../shared/report-role-filter.helper';

@Component({
  selector: 'app-top-commenters-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    TranslateModule,
    GenericTableComponent,
    PageHeaderComponent,
    DatePickerComponent,
    ReportRoleFilterComponent
  ],
  templateUrl: './top-commenter-list.component.html',
  styleUrls: ['./top-commenter-list.component.scss']
})
export class TopCommenterListComponent implements OnInit {

  @ViewChild(ReportRoleFilterComponent) roleFilter?: ReportRoleFilterComponent;

  title = 'REPORTS.TOP_COMMENTERS';
  activeitem = 'EMPLOYEE.LIST_TITLE';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.TOP_COMMENTERS'];

  // Table Columns
  columns: TableColumn[] = [
    { key: 'employeeName', label: 'REPORTS.EMPLOYEE_NAME' },
    { key: 'commentsCount', label: 'REPORTS.COMMENTS_COUNT' },
    { key: 'distinctTasksCount', label: 'REPORTS.DISTINCT_TASKS_COUNT' },
    { key: 'avgCommentsPerTask', label: 'REPORTS.AVG_COMMENTS_PER_TASK' },
    { key: 'lastCommentDate', label: 'REPORTS.LAST_COMMENT_DATE', type: 'dateTime' },];

  rows: EmployeeCommentsReportDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  fromDate?: string;
  toDate?: string;
  selectedRoleId?: number;

  isLoading = false;

  constructor(
    private reportService: ReportListService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private reportPdfService: ReportPdfService,

  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;

    const { roleId, roleTitle } = getReportRoleFilterParams(this.roleFilter, this.selectedRoleId);
    this.reportService.getTopCommenters(this.fromDate, this.toDate, roleId, roleTitle).subscribe({
      next: (res) => {
        this.rows = res.data;
        this.totalItems = res.data.length;
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

  


  onExportPdf(): void {
  const { roleId, roleTitle } = getReportRoleFilterParams(this.roleFilter, this.selectedRoleId);
  this.reportPdfService.getTopCommentersPdf(ExportType.Pdf, this.fromDate, this.toDate, roleId, roleTitle).subscribe({
    next: (blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'top-commenters-report.pdf';
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
 onExportExcel(): void {
  const { roleId, roleTitle } = getReportRoleFilterParams(this.roleFilter, this.selectedRoleId);
  this.reportPdfService.getTopCommentersPdf(ExportType.Excel, this.fromDate, this.toDate, roleId, roleTitle).subscribe({
    next: (blob) => {
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'top-commenters-report.xlsx';
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
}