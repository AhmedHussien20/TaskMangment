import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import {
  GenericTableComponent,
  TableColumn,
} from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { ReportListService } from 'app/core/services/report-list.service';
import { ReportPdfService } from 'app/core/services/report-pdf.service';
import { SimpleEmployee } from 'app/core/models/task/task';
import { EmployeeService } from 'app/core/services/employee.service';
import {
  ExportType,
  TaskMovementReportDto,
  TaskMovementType,
} from 'app/core/models/reports/reports';
import { AuthService } from 'app/core/services/auth.service';
import { EmployeeNgSelectComponent } from 'app/components/employee-select/employee-select.component';

@Component({
  selector: 'app-task-today-activity-report',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    TranslateModule,
    GenericTableComponent,
    PageHeaderComponent,
    EmployeeNgSelectComponent,
  ],
  templateUrl: './task-today-activity-report.component.html',
})
export class TaskTodayActivityReportComponent implements OnInit {
  title = 'REPORTS.TASK_MOVEMENT_TITLE';
  activeitem = 'REPORTS.TASK_MOVEMENT_TITLE';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.TASK_MOVEMENT_TITLE'];

  columns: TableColumn[] = [
    { key: 'taskTitleWithId', label: 'REPORTS.TASK' },
    { key: 'assignedBy', label: 'REPORTS.ASSIGNED_BY' },
    { key: 'assignedToText', label: 'TASK.ASSIGNED_TO' },
    { key: 'commentedBy', label: 'REPORTS.COMMENTED_BY' },
    { key: 'commentDate', label: 'REPORTS.COMMENT_DATE', type: 'dateTime' },
    { key: 'commentText', label: 'REPORTS.COMMENT' },
  ];
  exportType = ExportType.Pdf;
  rows: TaskMovementReportDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  employees: SimpleEmployee[] = [];
  selectedEmployeeId?: number;

  movementType: TaskMovementType = TaskMovementType.Outgoing;
  reportTitle?: string;

  isLoading = false;
  isAdmin = false;

  movementOptions = [
    { label: 'REPORTS.OUTGOING', value: TaskMovementType.Outgoing },
    { label: 'REPORTS.INCOMING', value: TaskMovementType.Incoming },
  ];

  constructor(
    private reportService: ReportListService,
    private reportPdfService: ReportPdfService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private employeeService: EmployeeService,
    private authService: AuthService,
  ) {}

  ngOnInit(): void {
    const roleLevel = this.authService.getRoleLevel() ?? 0;
    this.isAdmin = roleLevel >= 50;

    if (this.isAdmin) {
      this.selectedEmployeeId = undefined;
      this.loadData();
    } else {
      const user = this.authService.getCurrentUser();
      this.selectedEmployeeId = user.userId;
      this.loadData();
    }
  }

  loadData(): void {
    //if (!this.selectedEmployeeId) return;

    this.isLoading = true;

    this.reportService
      .getMovementReports(
        this.selectedEmployeeId,
        this.movementType,
        this.reportTitle,
      )
      .subscribe({
        next: (res) => {
          this.rows = (res.data || []).map((r) => ({
            ...r,
            assignedToText: (r.assignedTo || [])
              .map((x) => x.fullName)
              .join('، '),
          }));

          this.totalItems = this.rows.length;
          this.isLoading = false;
        },

        error: () => {
          this.isLoading = false;
          this.toastr.error(
            this.translate.instant('COMMON.ERROR_LOADING_DATA'),
          );
        },
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
    //if (!this.selectedEmployeeId) return;

    this.reportPdfService
      .getTaskMovementReportsPdf(
        this.exportType,
        this.selectedEmployeeId,
        this.movementType,
        this.reportTitle,
      )
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'task-movement-report.pdf';
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
        },
        error: () => {
          this.toastr.error(
            this.translate.instant('COMMON.ERROR_LOADING_DATA'),
          );
        },
      });
  }
  onExportExcel(): void {
    //if (!this.selectedEmployeeId) return;
    this.reportPdfService
      .getTaskMovementReportsPdf(
        ExportType.Excel,
        this.selectedEmployeeId,
        this.movementType,
        this.reportTitle,
      )
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'task-movement-report.xlsx';
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
        },
        error: () => {
          this.toastr.error(
            this.translate.instant('COMMON.ERROR_LOADING_DATA'),
          );
        },
      });
  }
}