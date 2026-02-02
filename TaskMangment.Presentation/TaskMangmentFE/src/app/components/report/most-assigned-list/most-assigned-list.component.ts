import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericTableComponent } from '../../../shared/components/generic-table/generic-table.component';
import { ReportListService } from 'app/core/services/report-list.service';
import { ReportPdfService } from 'app/core/services/report-pdf.service';
import { EmployeeAssignmentsReportDto } from 'app/core/models/reports/reports';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';

@Component({
  selector: 'app-most-assigned-list',
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
  templateUrl: './most-assigned-list.component.html',
  styleUrls: ['./most-assigned-list.component.scss']
})
export class MostAssignedListComponent implements OnInit {

  title = 'REPORTS.MOST_ASSIGNED';
  activeitem = 'EMPLOYEE.LIST_TITLE';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.MOST_ASSIGNED'];

  columns = [
    { key: 'employeeName', label: 'REPORTS.EMPLOYEE_NAME' },
    { key: 'totalTasks', label: 'REPORTS.TASKS_COUNT' },
    { key: 'newTasks', label: 'REPORTS.NEW_TASKS' },
    { key: 'inProgressTasks', label: 'REPORTS.IN_PROGRESS_TASKS' },
    { key: 'closedTasks', label: 'REPORTS.CLOSED_TASKS' },
    { key: 'overdueTasks', label: 'REPORTS.OVERDUE_TASKS' },
    { key: 'closingSoonTasks', label: 'REPORTS.CLOSING_SOON_TASKS' },
    { key: 'completionRate', label: 'REPORTS.COMPLETION_RATE' },
  ];
 
  rows: EmployeeAssignmentsReportDto[] = [];
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
    this.reportService.getMostAssigned(this.fromDate, this.toDate).subscribe({
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

  onPageChange(page: number): void {
    this.page = page;
  }

  onEntriesChange(entries: number): void {
    this.entries = entries;
    this.page = 1;
  }

  onExportPdf(): void {
    this.reportPdfService.getMostAssignedPdf(this.fromDate, this.toDate).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'most-assigned-report.pdf';
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
