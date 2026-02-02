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
import { TaskDiscountReportDto, TaskMovementType } from 'app/core/models/reports/reports';
import { SimpleEmployee } from 'app/core/models/task/task';
import { EmployeeService } from 'app/core/services/employee.service';
import { AuthService } from 'app/core/services/auth.service';
import { EmployeeNgSelectComponent } from 'app/components/employee-select/employee-select.component';
import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';

@Component({
  selector: 'app-tasks-discount-report',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    TranslateModule,
    GenericTableComponent,
    PageHeaderComponent,
    EmployeeNgSelectComponent,
    DatePickerComponent
  ],
  templateUrl: './tasks-discoun-report.component.html',
})
export class TasksDiscountReportComponent implements OnInit {

  title = 'REPORTS.TASKS_DISCOUNT';
  activeitem = 'REPORTS.TASKS_DISCOUNT';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS', 'REPORTS.TASKS_DISCOUNT'];

  columns: TableColumn[] = [
    { key: 'taskIdTitle', label: 'REPORTS.TASK' }, 
    { key: 'status', label: 'REPORTS.STATUS' },
    { key: 'assignedBy', label: 'REPORTS.ASSIGNED_BY' },
    { key: 'employeeName', label: 'REPORTS.EMPLOYEES' },
    { key: 'closedDate', label: 'REPORTS.CLOSED_DATE' , type: 'date' },
    { key: 'autoDiscount', label: 'REPORTS.AUTO_DISCOUNT' },
    { key: 'manualDiscount', label: 'REPORTS.MANUAL_DISCOUNT' },
    { key: 'evaluation', label: 'REPORTS.EVALUATION' }
  ];
employees: SimpleEmployee[] = [];
selectedEmployeeId?: number;

  rows: any[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  fromDate?: string;
  toDate?: string;
  status?: string;
  isAdmin = false;
  isLoading = false;
  movementType: TaskMovementType = TaskMovementType.Outgoing;
  
  movementOptions = [
      { label: 'REPORTS.OUTGOING', value: TaskMovementType.Outgoing },
      { label: 'REPORTS.INCOMING', value: TaskMovementType.Incoming }
    ];
 
 statusOptions = [
  { label: 'TASK.STATUS_IN_PROGRESS', value: 'InProgress' },
  { label: 'TASK.STATUS_ARCHIVED', value: 'Archived' },
  { label: 'TASK.STATUS_AUTOCLOSE', value: 'AutoClose' },
  { label: 'TASK.STATUS_CLOSED', value: 'Closed' }

];




  constructor(
    private reportService: ReportListService,
    private reportPdfService: ReportPdfService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private employeeService: EmployeeService,
      private authService: AuthService

  ) {}

  ngOnInit(): void {
  const roleLevel = this.authService.getRoleLevel();
  this.isAdmin = roleLevel >= 50;

  if (this.isAdmin) {
    this.loadEmployees();    
    this.selectedEmployeeId = undefined;
    this.loadData();
  } else {
    const user = this.authService.getCurrentUser();
    this.selectedEmployeeId = user.userId;
    this.loadData();             
  }
}



loadData(): void {
  if (!this.isAdmin && !this.selectedEmployeeId) {  
    return; 
  }

  this.isLoading = true;

  this.reportService.getTaskDiscounts(
    this.selectedEmployeeId!,
    this.movementType,
    this.fromDate ?? '',
    this.toDate,
    this.status
  ).subscribe({
    next: (res) => {
      this.rows = res.data.map((t: TaskDiscountReportDto) => ({
        ...t,
        taskIdTitle: `[${t.taskId}] ${t.title}`,
        assignedBy: t.assignedBy,
        employeeName: t.employeeName

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

loadEmployees(): void {
  const request = {
    searchKey: '',
    pageIndex: 1,
    pageSize: 1000,
    sortColumn: 'Id',
    sortDirection: 'DESC'
  };

  this.employeeService.getAll(request).subscribe({
    next: (res) => {
      this.employees = res.data.data.map((e: any) => ({
        id: e.id,
        fullName: e.fullName
      }));
    },
    error: () => {
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
      .getTaskDiscountsPdf(  this.selectedEmployeeId ?? 0, this.movementType,this.fromDate, this.toDate, this.status)
      .subscribe({
        next: (blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = 'task-discounts-report.pdf';
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
