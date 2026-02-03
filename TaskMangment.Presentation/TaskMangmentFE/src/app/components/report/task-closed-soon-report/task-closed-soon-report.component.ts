// task-closed-soon-report.component.ts
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
import { SimpleEmployee } from 'app/core/models/task/task';
import { EmployeeService } from 'app/core/services/employee.service';
import { ExportType, TasksClosingSoonDto } from 'app/core/models/reports/reports';
import { AuthService } from 'app/core/services/auth.service';
import { EmployeeNgSelectComponent } from 'app/components/employee-select/employee-select.component';

@Component({
  selector: 'app-task-closed-soon-report',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    TranslateModule,
    GenericTableComponent,
    PageHeaderComponent,
    EmployeeNgSelectComponent
  ],
  templateUrl: './task-closed-soon-report.component.html',
})
export class TaskClosedSoonReportComponent implements OnInit {

  title = 'REPORTS.TASKS_CLOSING_SOON';
  activeitem = 'REPORTS.TASKS_CLOSING_SOON_TITLE';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.TASKS_CLOSING_SOON'];

  columns: TableColumn[] = [
    { key: 'taskIdTitle', label: 'REPORTS.TASK' },
    { key: 'assignedBy', label: 'REPORTS.ASSIGNED_BY' },
   // { key: 'closedDate', label: 'REPORTS.CLOSED_DATE', type: 'date' },
    {key: 'dueDate', label: 'REPORTS.DUE_DATE', type: 'date' },
    { key: 'branchName', label: 'REPORTS.BRANCH_NAME' },
    { key: 'areaName', label: 'REPORTS.AREA_NAME' },
    { key: 'companyName', label: 'REPORTS.COMPANY_NAME' }
  ];
  employees: SimpleEmployee[] = [];
  selectedEmployeeId?: number;

  rows: TasksClosingSoonDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  fromDate?: string;
  toDate?: string;

  isLoading = false;
  isAdmin = false;

  constructor(
    private reportService: ReportListService,
    private reportPdfService: ReportPdfService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private employeeService: EmployeeService,
    private authService: AuthService
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

    this.isLoading = true;
    this.reportService.getClosedSoonReports(this.selectedEmployeeId).subscribe({
      next: (res) => {
        this.rows = res.data.map(t => ({
          ...t,
          taskIdTitle: `[${t.taskId}] ${t.title}`
        }));
        this.totalItems = this.rows.length;
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
    //if (!this.selectedEmployeeId) return;
    this.reportPdfService.getTaskClosedSoonReportsPdf(ExportType.Pdf, this.selectedEmployeeId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'tasks-closing-soon-report.pdf';
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
    //if (!this.selectedEmployeeId) return;
    this.reportPdfService.getTaskClosedSoonReportsPdf(ExportType.Excel, this.selectedEmployeeId).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'tasks-closing-soon-report.xlsx';
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
