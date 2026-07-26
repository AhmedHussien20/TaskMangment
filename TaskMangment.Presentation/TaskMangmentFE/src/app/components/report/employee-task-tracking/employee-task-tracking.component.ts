import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';
import { EmployeeNgSelectComponent } from 'app/components/employee-select/employee-select.component';

import { ReportListService } from 'app/core/services/report-list.service';
import { ReportPdfService } from 'app/core/services/report-pdf.service';
import { AuthService } from 'app/core/services/auth.service';
import { Permissions } from 'app/core/constants/permissions';

import { EmployeeTaskTrackingReportDto, ExportType } from 'app/core/models/reports/reports';
import { TaskDetailsShellComponent } from 'app/components/tasks/task-details/task-details-shell/task-details-shell.component';

@Component({
  selector: 'app-employee-task-tracking',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    TranslateModule,
    GenericTableComponent,
    PageHeaderComponent,
    DatePickerComponent,
    EmployeeNgSelectComponent
  ],
  templateUrl: './employee-task-tracking.component.html',
  styleUrls: ['./employee-task-tracking.component.scss']
})
export class EmployeeTaskTrackingComponent implements OnInit {

  title = 'REPORTS.EMPLOYEE_TASK_TRACKING';
  activeitem = 'REPORTS.EMPLOYEE_TASK_TRACKING';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.EMPLOYEE_TASK_TRACKING'];

  columns: TableColumn[] = [
    { key: 'taskIdTitle', label: 'REPORTS.TASK' },
    { key: 'assignedBy', label: 'REPORTS.ASSIGNED_BY' },
    { key: 'employeeName', label: 'REPORTS.EMPLOYEE_NAME' },
    { key: 'status', label: 'REPORTS.STATUS' },
    { key: 'createdDate', label: 'REPORTS.CREATED_DATE', type: 'date' },
    { key: 'closedAt', label: 'REPORTS.CLOSED_DATE', type: 'date' },
  ];

rows: any[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  fromDate?: string;
  toDate?: string;

  isLoading = false;
  isAdmin = false;

  selectedEmployeeId?: number;

  constructor(
    private reportService: ReportListService,
    private reportPdfService: ReportPdfService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private authService: AuthService,
    private modalService: NgbModal
  ) {}

  ngOnInit(): void {
    this.isAdmin = this.authService.hasAnyPermission(
      Permissions.VIEW_SCOPED_REPORTS,
      Permissions.VIEW_COMPANY_REPORTS,
      Permissions.VIEW_SCOPED_TASKS,
      Permissions.VIEW_COMPANY_TASKS,
      Permissions.VIEW_EMPLOYEES,
      Permissions.CREATE_TASK
    ) || this.authService.hasAccessScope();

    const user = this.authService.getCurrentUser();

    if (!this.isAdmin) {
      this.selectedEmployeeId = user.userId;
    } else {
      this.selectedEmployeeId = undefined;
    }
  }

  private ensureEmployeeSelectedIfAdmin(): boolean {
    if (this.isAdmin && (!this.selectedEmployeeId || this.selectedEmployeeId <= 0)) {
      this.toastr.error(this.translate.instant('REPORTS.EMPLOYEE_REQUIRED'));
      return false;
    }
    return true;
  }

  loadData(): void {
    if (!this.ensureEmployeeSelectedIfAdmin()) return;

    if (!this.fromDate) {
      this.rows = [];
      this.totalItems = 0;
      return;
    }
    this.isLoading = true;
    this.reportService.getEmployeeTaskTracking(this.selectedEmployeeId!, this.fromDate, this.toDate)
      .subscribe({
        next: (res) => {
this.rows = (res.data ?? []).map((t: EmployeeTaskTrackingReportDto) => ({
  id: t.taskId,
  ...t,
  taskIdTitle: `[${t.taskId}] ${t.title}`
}));
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
    if (!this.ensureEmployeeSelectedIfAdmin()) return;

    if (!this.fromDate) {
      this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
      return;
    }

    this.reportPdfService.getEmployeeTaskTrackingPdf(
      ExportType.Pdf,
      this.selectedEmployeeId!,
      this.fromDate,
      this.toDate
    ).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'employee-task-tracking.pdf';
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
    if (!this.ensureEmployeeSelectedIfAdmin()) return;

    if (!this.fromDate) {
      this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
      return;
    }

    this.reportPdfService.getEmployeeTaskTrackingPdf(
      ExportType.Excel,
      this.selectedEmployeeId!,
      this.fromDate,
      this.toDate
    ).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'employee-task-tracking.xlsx';
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
  checkRowClickable(item: any): boolean {
  console.log('Row:', item);    
  return true;
}
onEdit(id: number) {
  console.log('Editing task ID:', id); 
  const task = this.rows.find(x => x.taskId === id);
  console.log('Task object:', task);

  const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      backdrop: 'static',
      scrollable: true
    });

    modalRef.componentInstance.taskId = id;
    modalRef.componentInstance.readonly = true;
  }
}
