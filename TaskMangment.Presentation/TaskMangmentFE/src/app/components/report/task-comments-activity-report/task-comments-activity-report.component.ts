import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { ReportListService } from 'app/core/services/report-list.service';
import { ReportPdfService } from 'app/core/services/report-pdf.service';
import { TaskActivityReportDto } from 'app/core/models/reports/reports';
import { ApiResponse } from 'app/core/models/event/calendar';
import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';

@Component({
  selector: 'app-task-comments-activity-report',
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
  templateUrl: './task-comments-activity-report.component.html',
  styleUrls: ['./task-comments-activity-report.component.scss']
})
export class TaskCommentsActivityReportComponent implements OnInit {

  title = 'REPORTS.TASK_ACTIVITY_REPORT';
  activeitem = 'REPORTS.TASK_ACTIVITY_REPORT';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.TASK_ACTIVITY_REPORT'];

  columns: TableColumn[] = [
    { key: 'taskTitleWithId', label: 'REPORTS.TASK' },
    { key: 'assignedBy', label: 'REPORTS.ASSIGNED_BY' },
    { key: 'commentDate', label: 'REPORTS.COMMENT_DATE' , type : 'date'},
    { key: 'commentedBy', label: 'REPORTS.COMMENTED_BY'}
  ];

  rows: TaskActivityReportDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  fromDate?: string;
  toDate?: string | null;

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

    this.reportService
      .getTaskActivities(this.fromDate!, this.toDate!)
      .subscribe({
        next: (res: ApiResponse<TaskActivityReportDto[]>) => {
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
    this.reportPdfService
      .getTaskActivitiesPdf(this.fromDate!, this.toDate!)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'task-activities-report.pdf';
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
