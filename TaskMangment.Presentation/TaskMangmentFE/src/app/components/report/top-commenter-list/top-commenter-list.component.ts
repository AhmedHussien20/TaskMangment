import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericTableComponent } from '../../../shared/components/generic-table/generic-table.component';
import { ReportListService } from 'app/core/services/report-list.service';
import { EmployeeCommentsReportDto } from 'app/core/models/reports/reports';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { ReportPdfService } from 'app/core/services/report-pdf.service';

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
    
  ],
  templateUrl: './top-commenter-list.component.html',
  styleUrls: ['./top-commenter-list.component.scss']
})
export class TopCommenterListComponent implements OnInit {

  title = 'REPORTS.TOP_COMMENTERS';
  activeitem = 'EMPLOYEE.LIST_TITLE';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.TOP_COMMENTERS'];

  // Table Columns
  columns = [
    { key: 'employeeName', label: 'REPORTS.EMPLOYEE_NAME' },
    { key: 'commentsCount', label: 'REPORTS.COMMENTS_COUNT' },
    { key: 'distinctTasksCount', label: 'REPORTS.DISTINCT_TASKS_COUNT' },
    { key: 'avgCommentsPerTask', label: 'REPORTS.AVG_COMMENTS_PER_TASK' },
    { key: 'lastCommentDate', label: 'REPORTS.LAST_COMMENT_DATE' },];

  rows: EmployeeCommentsReportDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  fromDate?: string;
  toDate?: string;

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

    this.reportService.getTopCommenters(this.fromDate, this.toDate).subscribe({
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
  this.reportPdfService.getTopCommentersPdf(this.fromDate, this.toDate).subscribe({
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
}
