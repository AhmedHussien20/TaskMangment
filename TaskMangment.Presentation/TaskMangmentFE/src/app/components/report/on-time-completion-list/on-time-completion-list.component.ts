import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { ReportListService } from 'app/core/services/report-list.service';
import { ReportPdfService } from 'app/core/services/report-pdf.service';
import { EmployeeOnTimeReportDto } from 'app/core/models/reports/reports';
import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';

@Component({
  selector: 'app-on-time-completion-list',
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
  templateUrl: './on-time-completion-list.component.html',
  styleUrls: ['./on-time-completion-list.component.scss']
})
export class OnTimeCompletionListComponent implements OnInit {

  title = 'REPORTS.ON_TIME_COMPLETION';
  activeitem = 'EMPLOYEE.LIST_TITLE';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.ON_TIME_COMPLETION'];

  columns = [
    { key: 'employeeName', label: 'REPORTS.EMPLOYEE_NAME' },
    { key: 'onTimeTasks', label: 'REPORTS.ON_TIME_COUNT' },
    { key: 'totalClosedTasks', label: 'REPORTS.TASKS_CLOSED_COUNT' },
    { key: 'lateTasks', label: 'REPORTS.LATE_TASKS_COUNT' },
    { key: 'commitmentPercentage', label: 'REPORTS.COMMITMENT_PERCENTAGE' },
  ];

  rows: EmployeeOnTimeReportDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  fromDate?: string;
  toDate?: string;

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
      .getOnTimeCompletion(this.fromDate, this.toDate)
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
    this.reportPdfService
      .getOnTimeCompletionPdf(this.fromDate, this.toDate)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'on-time-completion-report.pdf';
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
