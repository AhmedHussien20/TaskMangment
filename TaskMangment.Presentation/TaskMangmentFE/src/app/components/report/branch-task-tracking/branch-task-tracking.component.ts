import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';

import { ReportListService } from 'app/core/services/report-list.service';
import { ReportPdfService } from 'app/core/services/report-pdf.service';

import { ExportType, BranchTaskReportRowDto } from 'app/core/models/reports/reports';
import { BranchService } from 'app/core/services/branch.service';

@Component({
  selector: 'app-branch-task-tracking',
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
  templateUrl: './branch-task-tracking.component.html',
  styleUrls: ['./branch-task-tracking.component.scss']
})
export class BranchTaskTrackingComponent implements OnInit {

  title = 'REPORTS.BRANCH_TASK_TRACKING';
  activeitem = 'REPORTS.BRANCH_TASK_TRACKING';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.BRANCH_TASK_TRACKING'];

  columns: TableColumn[] = [
    { key: 'taskIdTitle', label: 'REPORTS.TASK' },
    { key: 'assignedBy', label: 'REPORTS.ASSIGNED_BY' },

    { key: 'employeesText', label: 'TASK.ASSIGNED_TO' },

    { key: 'statusText', label: 'REPORTS.STATUS' },
    { key: 'createdDate', label: 'REPORTS.CREATED_DATE', type: 'date' },
    { key: 'effectiveDueDate', label: 'REPORTS.DUE_DATE', type: 'date' },

    { key: 'extensionRequestsCount', label: 'TASK.EXTENSIONS_COUNT' },
  ];

  rows: any[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  fromDate?: string;
  toDate?: string;

  isLoading = false;

  // ✅ Branch filter
  selectedBranchId?: number;
  branches: { id: number; name: string }[] = [];

  constructor(
    private reportService: ReportListService,
    private reportPdfService: ReportPdfService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private branchService :BranchService
  ) {}

  ngOnInit(): void {
    this.loadBranches()
  }
loadBranches(): void {
  const req = {
    searchKey: '',
    pageIndex: 1,
    pageSize: 500,
    sortColumn: 'Id',
    sortDirection: 'DESC'
  };

  this.branchService.getAll(req).subscribe({
    next: (res) => {
      const list = res?.data?.data ?? [];

      this.branches = list.map((b: any) => ({
        id: Number(b.id),
        name: b.name
      }));
    },
    error: () => {
      this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
    }
  });
}

  private ensureBranchSelected(): boolean {
    if (!this.selectedBranchId || this.selectedBranchId <= 0) {
      this.toastr.error(this.translate.instant('REPORTS.BRANCH_REQUIRED'));
      return false;
    }
    return true;
  }

  loadData(): void {
    if (!this.ensureBranchSelected()) return;

    if (!this.fromDate) {
      this.rows = [];
      this.totalItems = 0;
      return;
    }

    this.isLoading = true;

    this.reportService.getBranchTasks(this.selectedBranchId!, this.fromDate, this.toDate)
      .subscribe({
        next: (res) => {
          const data = res.data ?? [];

          this.rows = data.map((t: BranchTaskReportRowDto) => ({
            ...t,

            taskIdTitle: `[${t.taskId}] ${t.title}`,

            employeesText: (t.employees && t.employees.length)
              ? t.employees.map(e => e.name).join('، ')
              : '-',

            isSharedText: t.isShared ? this.translate.instant('COMMON.YES') : this.translate.instant('COMMON.NO'),
            isMergedText: t.isMergedByTitle ? this.translate.instant('COMMON.YES') : this.translate.instant('COMMON.NO'),
            mergedCount: (t.mergedTaskIds?.length ?? 1),

            // تاريخ الانتهاء الفعلي
            effectiveDueDate: t.effectiveDueDate ?? null
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
    if (!this.ensureBranchSelected()) return;

    if (!this.fromDate) {
      this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
      return;
    }

    this.reportPdfService.getBranchTasksPdf(
      ExportType.Pdf,
      this.selectedBranchId!,
      this.fromDate,
      this.toDate
    ).subscribe({
      next: (blob) => this.downloadBlob(blob, 'branch-tasks-report.pdf'),
      error: () => this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'))
    });
  }

  onExportExcel(): void {
    if (!this.ensureBranchSelected()) return;

    if (!this.fromDate) {
      this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
      return;
    }

    this.reportPdfService.getBranchTasksPdf(
      ExportType.Excel,
      this.selectedBranchId!,
      this.fromDate,
      this.toDate
    ).subscribe({
      next: (blob) => this.downloadBlob(blob, 'branch-tasks-report.xlsx'),
      error: () => this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'))
    });
  }

  private downloadBlob(blob: Blob, filename: string) {
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }
}
