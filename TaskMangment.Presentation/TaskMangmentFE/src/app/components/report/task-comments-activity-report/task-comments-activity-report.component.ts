import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { ReportListService } from 'app/core/services/report-list.service';
import { ReportPdfService } from 'app/core/services/report-pdf.service';
import { ExportType, TaskActivityReportDto } from 'app/core/models/reports/reports';
import { ApiResponse } from 'app/core/models/event/calendar';
import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';
import { ReportRoleFilterComponent } from '../shared/report-role-filter.component';
import { getReportRoleFilterParams } from '../shared/report-role-filter.helper';
import { TaskDetailsShellComponent } from 'app/components/tasks/task-details/task-details-shell/task-details-shell.component';

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
    DatePickerComponent,
    ReportRoleFilterComponent
  ],
  templateUrl: './task-comments-activity-report.component.html',
  styleUrls: ['./task-comments-activity-report.component.scss']
})
export class TaskCommentsActivityReportComponent implements OnInit {

  @ViewChild(ReportRoleFilterComponent) roleFilter?: ReportRoleFilterComponent;

  title = 'REPORTS.TASK_ACTIVITY_REPORT';
  activeitem = 'REPORTS.TASK_ACTIVITY_REPORT';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.TASK_ACTIVITY_REPORT'];

  columns: TableColumn[] = [
    { key: 'taskTitleWithId', label: 'REPORTS.TASK' },
    { key: 'assignedBy', label: 'REPORTS.ASSIGNED_BY' },
    { key: 'commentDate', label: 'REPORTS.COMMENT_DATE' , type : 'date'},
    { key: 'commentedBy', label: 'REPORTS.COMMENTED_BY'},
    { key: 'comment', label: 'REPORTS.COMMENT' },
  ];
 exportType= ExportType.Pdf
  rows: TaskActivityReportDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  fromDate?: string;
  toDate?: string | null;
  selectedRoleId?: number;

  isLoading = false;

  constructor(
    private reportService: ReportListService,
    private reportPdfService: ReportPdfService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private modalService: NgbModal
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;

    const { roleId, roleTitle } = getReportRoleFilterParams(this.roleFilter, this.selectedRoleId);
    this.reportService
      .getTaskActivities(this.fromDate!, this.toDate!, roleId, roleTitle)
      .subscribe({
        next: (res: ApiResponse<TaskActivityReportDto[]>) => {
this.rows = res.data.map(t => ({
  id: t.taskId,
  taskId: t.taskId,
  taskTitleWithId: t.taskTitleWithId,
  assignedBy: t.assignedBy,
  commentDate: t.commentDate,
  commentedBy: t.commentedBy,
  comment: t.comment
}));          this.totalItems = res.data.length;
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
      .getTaskActivitiesPdf(this.fromDate!, this.toDate!, this.exportType, roleId, roleTitle)
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

  onExportExcel(): void {
  const { roleId, roleTitle } = getReportRoleFilterParams(this.roleFilter, this.selectedRoleId);
  this.reportPdfService
    .getTaskActivitiesPdf(this.fromDate!, this.toDate!, ExportType.Excel, roleId, roleTitle)
    .subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'task-activities-report.xlsx';
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
