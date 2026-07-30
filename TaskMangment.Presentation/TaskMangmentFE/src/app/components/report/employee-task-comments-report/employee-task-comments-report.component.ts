import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { EmployeeNgSelectComponent } from 'app/components/employee-select/employee-select.component';
import { TaskDetailsShellComponent } from 'app/components/tasks/task-details/task-details-shell/task-details-shell.component';

import { ReportListService } from 'app/core/services/report-list.service';
import { ReportPdfService } from 'app/core/services/report-pdf.service';
import { AuthService } from 'app/core/services/auth.service';
import { Permissions } from 'app/core/constants/permissions';

import { EmployeeAssignedTaskOptionDto, EmployeeTaskCommentRowDto, ExportType } from 'app/core/models/reports/reports';

@Component({
  selector: 'app-employee-task-comments-report',
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
  templateUrl: './employee-task-comments-report.component.html',
  styleUrls: ['./employee-task-comments-report.component.scss'],
})
export class EmployeeTaskCommentsReportComponent implements OnInit {
  title = 'REPORTS.EMPLOYEE_TASK_COMMENTS';
  activeitem = 'REPORTS.EMPLOYEE_TASK_COMMENTS';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.EMPLOYEE_TASK_COMMENTS'];

  columns: TableColumn[] = [
    { key: 'commentDate', label: 'REPORTS.COMMENT_DATE', type: 'dateTime' },
    { key: 'commentText', label: 'REPORTS.COMMENT' },
  ];

  rows: any[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  isLoading = false;
  isAdmin = false;

  selectedEmployeeId?: number;
  selectedTaskId?: number;
  tasks: EmployeeAssignedTaskOptionDto[] = [];

  constructor(
    private reportService: ReportListService,
    private reportPdfService: ReportPdfService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private authService: AuthService,
    private modalService: NgbModal,
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
      this.onEmployeeChange();
    }
  }

  onEmployeeChange(): void {
    this.selectedTaskId = undefined;
    this.tasks = [];
    this.rows = [];
    this.totalItems = 0;

    if (!this.selectedEmployeeId) return;

    this.reportService.getEmployeeAssignedTasks(this.selectedEmployeeId).subscribe({
      next: (res) => {
        this.tasks = res.data ?? [];
      },
      error: () => {
        this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
      },
    });
  }

  private ensureEmployeeSelectedIfAdmin(): boolean {
    if (this.isAdmin && (!this.selectedEmployeeId || this.selectedEmployeeId <= 0)) {
      this.toastr.error(this.translate.instant('REPORTS.EMPLOYEE_REQUIRED'));
      return false;
    }
    return true;
  }

  private ensureTaskSelected(): boolean {
    if (!this.selectedTaskId || this.selectedTaskId <= 0) {
      this.toastr.error(this.translate.instant('REPORTS.TASK_REQUIRED'));
      return false;
    }
    return true;
  }

  loadData(): void {
    if (!this.ensureEmployeeSelectedIfAdmin()) return;
    if (!this.selectedEmployeeId) return;
    if (!this.ensureTaskSelected()) return;

    this.isLoading = true;
    this.reportService.getEmployeeTaskComments(this.selectedEmployeeId, this.selectedTaskId!).subscribe({
      next: (res) => {
        const data = res.data ?? [];
        this.rows = data.map((x: EmployeeTaskCommentRowDto) => ({
          id: x.commentId,
          ...x,
        }));
        this.totalItems = this.rows.length;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
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
    if (!this.ensureEmployeeSelectedIfAdmin()) return;
    if (!this.selectedEmployeeId) return;
    if (!this.ensureTaskSelected()) return;

    this.reportPdfService
      .getEmployeeTaskCommentsPdf(ExportType.Pdf, this.selectedEmployeeId, this.selectedTaskId!)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'employee-task-comments.pdf';
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
        },
        error: () => {
          this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
        },
      });
  }

  onExportExcel(): void {
    if (!this.ensureEmployeeSelectedIfAdmin()) return;
    if (!this.selectedEmployeeId) return;
    if (!this.ensureTaskSelected()) return;

    this.reportPdfService
      .getEmployeeTaskCommentsPdf(ExportType.Excel, this.selectedEmployeeId, this.selectedTaskId!)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'employee-task-comments.xlsx';
          document.body.appendChild(a);
          a.click();
          document.body.removeChild(a);
          window.URL.revokeObjectURL(url);
        },
        error: () => {
          this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
        },
      });
  }

  checkRowClickable(item: any): boolean {
    return true;
  }

  onEdit(id: number) {
    const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      windowClass: 'task-details-modal',
      backdrop: 'static',
      scrollable: true
    });

    modalRef.componentInstance.taskId = this.selectedTaskId ?? id;
    modalRef.componentInstance.readonly = true;
  }
}

