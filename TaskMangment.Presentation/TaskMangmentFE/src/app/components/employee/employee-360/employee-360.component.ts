import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import {
  NgbDropdownModule,
  NgbModal,
  NgbModalModule,
  NgbNavModule,
  NgbPaginationModule
} from '@ng-bootstrap/ng-bootstrap';
import { NgApexchartsModule } from 'ng-apexcharts';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { TaskCreateUpdateComponent } from 'app/components/tasks/task-create-update/task-create-update.component';
import { EmployeeCreateUpdateComponent } from 'app/components/employee/employee-create-update/employee-create-update.component';
import { TaskDetailsShellComponent } from 'app/components/tasks/task-details/task-details-shell/task-details-shell.component';
import {
  Employee360TaskFilter,
  Employee360TasksPopupComponent
} from './employee-360-tasks-popup.component';
import { EmployeeService } from 'app/core/services/employee.service';
import { TaskService } from 'app/core/services/task.service';
import { LeaveService } from 'app/core/services/leave.service';
import { AuthService } from 'app/core/services/auth.service';
import { Permissions } from 'app/core/constants/permissions';
import Swal from 'sweetalert2';
import {
  Employee360AccessDto,
  Employee360CommentItem,
  Employee360Deadline,
  Employee360Dto,
  Employee360EmailItem,
  Employee360LeaveDto,
  Employee360NotificationItem,
  Employee360PerformanceDto,
  EmployeeDeductionDto,
  EmployeeTimelineItem,
  EmployeeWarningDto
} from 'app/core/models/employee/employee-360';
import { AttachmentVm } from 'app/core/models/task/task-comment';
import { TaskGet } from 'app/core/models/task/task';
import { MyDatePipe } from 'app/components/utilities/pipline/MyDatePipe';
import { DatePickerComponent } from 'app/components/date-picker/date-picker.component';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { TaskCommentService } from 'app/core/services/task-comment.service';
import {
  ApexAxisChartSeries,
  ApexChart,
  ApexDataLabels,
  ApexLegend,
  ApexNonAxisChartSeries,
  ApexPlotOptions,
  ApexResponsive,
  ApexXAxis
} from 'ng-apexcharts';

export type ChartOptions = {
  series: ApexAxisChartSeries | ApexNonAxisChartSeries;
  chart: ApexChart;
  labels?: string[];
  xaxis?: ApexXAxis;
  dataLabels?: ApexDataLabels;
  legend?: ApexLegend;
  plotOptions?: ApexPlotOptions;
  colors?: string[];
  responsive?: ApexResponsive[];
};

@Component({
  selector: 'app-employee-360',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterModule,
    TranslateModule,
    NgbModalModule,
    NgbNavModule,
    NgbPaginationModule,
    NgbDropdownModule,
    NgApexchartsModule,
    PageHeaderComponent,
    GenericTableComponent,
    TaskCreateUpdateComponent,
    EmployeeCreateUpdateComponent,
    MyDatePipe,
    DatePickerComponent
  ],
  templateUrl: './employee-360.component.html',
  styleUrls: ['./employee-360.component.scss']
})
export class Employee360Component implements OnInit {
  employeeId!: number;
  activeTab = 'overview';
  isLoading = false;
  data: Employee360Dto | null = null;

  canCreateTask = false;
  canEditEmployee = false;
  canApproveLeave = false;
  canRejectLeave = false;

  /** Shared date range for KPIs, charts, sidebar, and all tabs */
  globalFrom = '';
  globalTo = '';
  /** True when dates are cleared and API is called without From/To (all data). */
  allPeriodActive = false;

  kpiCards: {
    key: string;
    label: string;
    value: string | number;
    icon: string;
    color: string;
    tab?: string;
    taskFilter?: Employee360TaskFilter;
    clickable?: boolean;
  }[] = [];

  statusChart: Partial<ChartOptions> = {};
  productivityChart: Partial<ChartOptions> = {};

  // Work
  taskRows: TaskGet[] = [];
  taskTotal = 0;
  taskPage = 1;
  taskPageSize = 10;
  taskSearch = '';
  taskStatusId: number | null = null;
  taskPriorityId: number | null = null;
  taskLoading = false;

  /** Default page size for client-side lists (discounts, leave, performance). */
  readonly listPageSize = 10;
  discountPage = 1;
  leavePage = 1;
  warningPage = 1;
  latePage = 1;

  taskColumns: TableColumn[] = [
    { key: 'id', label: 'TASK.ID' },
    { key: 'title', label: 'TASK.TITLE' },
    { key: 'assignedByName', label: 'TASK.ASSIGNED_BY' },
    {
      key: 'priorityText',
      label: 'TASK.PRIORITY',
      type: 'badge',
      badgeMap: {
        Low: { text: 'TASK.PRIORITY_LOW', class: 'bg-success' },
        Medium: { text: 'TASK.PRIORITY_MEDIUM', class: 'bg-warning' },
        High: { text: 'TASK.PRIORITY_HIGH', class: 'bg-danger' }
      }
    },
    {
      key: 'statusText',
      label: 'TASK.STATUS',
      type: 'badge',
      badgeMap: {
        New: { text: 'TASK.STATUS_NEW', class: 'bg-secondary' },
        InProgress: { text: 'TASK.STATUS_IN_PROGRESS', class: 'bg-info' },
        Closed: { text: 'TASK.STATUS_CLOSED', class: 'bg-success' },
        Archived: { text: 'TASK.STATUS_ARCHIVED', class: 'bg-dark' }
      }
    },
    { key: 'dueDate', label: 'TASK.DUE_DATE', type: 'date' }
  ];
  statusOptions = [
    { id: 1, name: 'TASK.STATUS_NEW' },
    { id: 2, name: 'TASK.STATUS_IN_PROGRESS' },
    { id: 3, name: 'TASK.STATUS_CLOSED' },
    { id: 4, name: 'TASK.STATUS_ARCHIVED' }
  ];
  priorityOptions = [
    { id: 1, name: 'TASK.PRIORITY_LOW' },
    { id: 2, name: 'TASK.PRIORITY_MEDIUM' },
    { id: 3, name: 'TASK.PRIORITY_HIGH' }
  ];

  access: Employee360AccessDto | null = null;
  accessLoading = false;
  accessSearch = '';

  performance: Employee360PerformanceDto | null = null;
  performanceLoading = false;
  performanceSearch = '';

  discountsData: EmployeeDeductionDto[] = [];
  discountsTotalAmount = 0;
  discountsLoading = false;
  discountSearch = '';

  leave: Employee360LeaveDto | null = null;
  leaveLoading = false;
  leaveSearch = '';

  emails: Employee360EmailItem[] = [];
  emailTotal = 0;
  emailPage = 1;
  emailPageSize = 15;
  emailLoading = false;
  emailSearch = '';
  emailStatus = '';
  emailType = '';
  emailTypeOptions = [
    'LeaveApproved',
    'LeaveRejected',
    'LeaveRequestCreated',
    'TaskDueTodayReminder',
    'TaskCommentAdded',
    'TaskAssignedToExistingTask',
    'TaskUnAssignedFromExistingTask',
    'TaskExtensionRequest',
    'TaskExtensionApproved',
    'TaskCloseRequest',
    'TaskCloseApproved',
    'TaskAchievement',
    'EmployeeDeduction',
    'EmployeeWarning',
    'MonthlyEmployeeDiscounts',
    'OfficialHoliday',
    'OfferSent',
    'DeveloperErrorAlert'
  ];

  notifications: Employee360NotificationItem[] = [];
  notifTotal = 0;
  notifPage = 1;
  notifPageSize = 15;
  notifLoading = false;
  notifSearch = '';
  notifStatus = '';
  notifChannel = '';
  notifUnreadOnly = false;

  comments: Employee360CommentItem[] = [];
  commentTotal = 0;
  commentPage = 1;
  commentPageSize = 15;
  commentLoading = false;
  commentSearch = '';
  openAttachmentsForCommentId: number | null = null;
  attachmentsMap: Record<number, AttachmentVm[]> = {};
  loadingAttachments: Record<number, boolean> = {};

  timeline: EmployeeTimelineItem[] = [];
  timelineTotal = 0;
  timelinePage = 1;
  timelinePageSize = 20;
  timelineLoading = false;
  timelineSearch = '';

  title = 'EMPLOYEE_360.TITLE';
  breadcrumbs = ['MENU.HOME', 'MENU.EMPLOYEES', 'EMPLOYEE.LIST_TITLE', 'EMPLOYEE_360.TITLE'];
  activeitem = 'EMPLOYEE_360.TITLE';
  modalKey = 0;
  employeeEditKey = 0;
  emptySearchCriteria: SearchCriteria = {
    searchKey: '',
    pageIndex: 1,
    pageSize: 10,
    sortColumn: 'Id',
    sortDirection: 'DESC'
  };

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private employeeService: EmployeeService,
    private taskService: TaskService,
    private leaveService: LeaveService,
    private commentService: TaskCommentService,
    private auth: AuthService,
    private modalService: NgbModal,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.employeeId = Number(this.route.snapshot.paramMap.get('id'));
    if (!this.employeeId) {
      this.router.navigate(['/employee/employee-list']);
      return;
    }
    this.canApproveLeave = this.auth.hasPermission(Permissions.APPROVE_LEAVE);
    this.canRejectLeave = this.auth.hasPermission(Permissions.REJECT_LEAVE);
    this.setDefaultMonthRange();
    this.load360();
  }

  get canReviewLeave(): boolean {
    return this.canApproveLeave || this.canRejectLeave;
  }

  /** First day of current month → today (yyyy-MM-dd). */
  private setDefaultMonthRange(): void {
    const now = new Date();
    this.globalFrom = this.formatDateYmd(new Date(now.getFullYear(), now.getMonth(), 1));
    this.globalTo = this.formatDateYmd(now);
    this.allPeriodActive = false;
  }

  private formatDateYmd(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  private dateRangeParams(): { from?: string; to?: string } {
    if (this.allPeriodActive) {
      return {};
    }
    return {
      from: this.globalFrom || undefined,
      to: this.globalTo || undefined
    };
  }

  applyGlobalDateRange(): void {
    this.allPeriodActive = !this.globalFrom && !this.globalTo;
    this.taskPage = 1;
    this.emailPage = 1;
    this.notifPage = 1;
    this.commentPage = 1;
    this.timelinePage = 1;
    this.discountPage = 1;
    this.leavePage = 1;
    this.warningPage = 1;
    this.latePage = 1;
    this.performance = null;
    this.access = null;
    this.discountsData = [];
    this.discountsTotalAmount = 0;
    this.leave = null;
    this.load360(false);
    this.onTabChange(this.activeTab);
  }

  /** Remove date filter and load all historical data. */
  applyFullPeriod(): void {
    this.globalFrom = '';
    this.globalTo = '';
    this.allPeriodActive = true;
    this.applyGlobalDateRange();
  }

  /** Restore default: current month day 1 → today. */
  resetToCurrentMonth(): void {
    this.setDefaultMonthRange();
    this.applyGlobalDateRange();
  }

  load360(showPageLoader = true): void {
    if (showPageLoader) this.isLoading = true;
    this.employeeService.get360(this.employeeId, this.dateRangeParams()).subscribe({
      next: (res) => {
        this.data = res.data;
        this.canCreateTask = !!res.data?.canCreateTask && this.auth.hasPermission('CREATE_TASK');
        this.canEditEmployee = !!res.data?.canEditEmployee || this.auth.hasPermission('UPDATE_EMPLOYEE');
        this.buildKpiCards();
        this.buildCharts();
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
        this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
        if (showPageLoader) {
          this.router.navigate(['/employee/employee-list']);
        }
      }
    });
  }

  buildKpiCards(): void {
    const k = this.data?.kpis;
    if (!k) {
      this.kpiCards = [];
      return;
    }
    this.kpiCards = [
      { key: 'active', label: 'EMPLOYEE_360.KPI_ACTIVE', value: k.activeTasks, icon: 'bi-list-task', color: 'kpi-blue', taskFilter: 'active', clickable: true },
      { key: 'completed', label: 'EMPLOYEE_360.KPI_COMPLETED', value: k.completedTasks, icon: 'bi-check2-circle', color: 'kpi-green', taskFilter: 'completed', clickable: true },
      { key: 'overdue', label: 'EMPLOYEE_360.KPI_OVERDUE', value: k.overdueTasks, icon: 'bi-exclamation-octagon', color: 'kpi-red', taskFilter: 'overdue', clickable: true },
      { key: 'week', label: 'EMPLOYEE_360.KPI_DUE_WEEK', value: k.dueThisWeek, icon: 'bi-calendar-week', color: 'kpi-orange', taskFilter: 'week', clickable: true },
      { key: 'rate', label: 'EMPLOYEE_360.KPI_COMPLETION', value: `${k.completionRate}%`, icon: 'bi-pie-chart', color: 'kpi-teal' },
      { key: 'warn', label: 'EMPLOYEE_360.KPI_WARNINGS', value: k.openWarnings, icon: 'bi-exclamation-triangle', color: 'kpi-amber', tab: 'performance', clickable: true },
      { key: 'disc', label: 'EMPLOYEE_360.KPI_TOTAL_DISCOUNTS', value: k.totalDiscounts, icon: 'bi-cash-coin', color: 'kpi-purple', tab: 'discounts', clickable: true },
      { key: 'leave', label: 'EMPLOYEE_360.KPI_LEAVE_BAL', value: k.leaveBalance, icon: 'bi-airplane', color: 'kpi-cyan', tab: 'leave', clickable: true },
      { key: 'load', label: 'EMPLOYEE_360.KPI_WORKLOAD', value: k.currentWorkload, icon: 'bi-speedometer2', color: 'kpi-indigo', taskFilter: 'active', clickable: true },
      { key: 'score', label: 'EMPLOYEE_360.KPI_SCORE', value: k.performanceScore, icon: 'bi-trophy', color: 'kpi-pink', tab: 'performance', clickable: true }
    ];
  }

  onKpiClick(card: { tab?: string; taskFilter?: Employee360TaskFilter; clickable?: boolean; label?: string }): void {
    if (!card.clickable) return;
    if (card.taskFilter) {
      this.openTasksPopup(card.taskFilter, card.label || 'TASK.LIST_TITLE');
      return;
    }
    if (card.tab) this.onTabChange(card.tab);
  }

  openTasksPopup(filter: Employee360TaskFilter, titleKey: string, linkedTasks?: Array<{
    id: number;
    title?: string;
    statusText?: string;
    assignedByName?: string;
    dueDate?: string | null;
    createdByMe?: boolean;
  }>): void {
    const modalRef = this.modalService.open(Employee360TasksPopupComponent, {
      size: 'xl',
      centered: true,
      scrollable: true
    });
    modalRef.componentInstance.titleKey = titleKey;
    modalRef.componentInstance.employeeId = this.employeeId;
    modalRef.componentInstance.filter = filter;
    modalRef.componentInstance.from = this.globalFrom;
    modalRef.componentInstance.to = this.globalTo;
    if (linkedTasks) {
      modalRef.componentInstance.linkedTasks = linkedTasks;
    }
  }

  openDiscountsWithTasks(): void {
    if (!this.discountsWithTaskCount) return;
    const linked = this.filteredDiscounts
      .filter(d => !!d.taskId)
      .map(d => ({
        id: d.taskId!,
        title: d.taskTitle || undefined,
        statusText: d.taskStatusText || undefined,
        assignedByName: d.taskAssignedByName || undefined,
        dueDate: d.taskDueDate ?? null
      }));
    const map = new Map<number, {
      id: number;
      title?: string;
      statusText?: string;
      assignedByName?: string;
      dueDate?: string | null;
    }>();
    linked.forEach(t => {
      if (!map.has(t.id)) map.set(t.id, t);
    });
    this.openTasksPopup('linked', 'EMPLOYEE_360.DISCOUNTS_WITH_TASK', Array.from(map.values()));
  }

  buildCharts(): void {
    const charts = this.data?.overviewCharts;
    const status = charts?.taskStatusBreakdown ?? [];
    this.statusChart = {
      series: status.map(s => s.count),
      chart: { type: 'donut', height: 260 },
      labels: status.map(s => s.name),
      colors: ['#6366f1', '#0ea5e9', '#22c55e', '#64748b', '#f59e0b'],
      legend: { position: 'bottom' },
      dataLabels: { enabled: true }
    };

    const monthly = charts?.monthlyProductivity ?? [];
    this.productivityChart = {
      series: [{ name: 'Completed', data: monthly.map(m => m.count) }],
      chart: { type: 'bar', height: 260, toolbar: { show: false } },
      xaxis: { categories: monthly.map(m => m.name) },
      colors: ['#0ea5e9'],
      plotOptions: { bar: { borderRadius: 4, columnWidth: '45%' } },
      dataLabels: { enabled: false }
    };
  }

  onTabChange(tab: string | number): void {
    this.activeTab = String(tab);
    switch (this.activeTab) {
      case 'work': this.loadTasks(); break;
      case 'access': this.loadAccess(); break;
      case 'performance': this.loadPerformance(); break;
      case 'discounts': this.loadDiscounts(); break;
      case 'leave': this.loadLeave(); break;
      case 'timeline': this.loadTimeline(); break;
      case 'emails': this.loadEmails(); break;
      case 'notifications': this.loadNotifications(); break;
      case 'comments': this.loadComments(); break;
    }
  }

  openTaskFromEvent(event: Event, taskId?: number | null): void {
    event.preventDefault();
    event.stopPropagation();
    if (taskId) this.openTaskDetails(taskId);
  }

  /** Same pattern as reports/dashboard: row click opens task details. */
  taskRowClickable = (_item: unknown): boolean => true;

  openTaskDetails(taskId?: number | null): void {
    if (!taskId) return;

    const fromRows = this.taskRows.find(x => x.id === taskId);
    const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      windowClass: 'task-details-modal',
      backdrop: 'static',
      scrollable: true
    });
    modalRef.componentInstance.taskId = taskId;
    modalRef.componentInstance.readonly = true;
    modalRef.componentInstance.createdByMe = fromRows?.createdByMe ?? false;
  }

  loadTasks(): void {
    this.taskLoading = true;
    const request: any = {
      searchKey: this.taskSearch || '',
      pageIndex: this.taskPage,
      pageSize: this.taskPageSize,
      sortColumn: 'Id',
      sortDirection: 'DESC',
      employeeIds: [this.employeeId],
      // 360 Work: show this employee's tasks in access scope (not intersect with "my tasks").
      viewScopedTasks: true
    };
    if (this.taskStatusId) request.statusId = this.taskStatusId;
    if (this.taskPriorityId) request.priorityId = this.taskPriorityId;
    if (this.globalFrom) request.createdFrom = this.globalFrom;
    if (this.globalTo) request.createdTo = this.globalTo;

    this.taskService.getAll(request).subscribe({
      next: (res: any) => {
        this.taskRows = res.data?.data ?? [];
        this.taskTotal = res.data?.totalCount ?? 0;
        this.taskLoading = false;
      },
      error: () => { this.taskLoading = false; }
    });
  }

  applyTaskFilters(): void {
    this.taskPage = 1;
    this.loadTasks();
  }

  onTaskPageChange(page: number): void {
    this.taskPage = page;
    this.loadTasks();
  }

  onTaskEntriesChange(entries: number): void {
    this.taskPageSize = entries;
    this.taskPage = 1;
    this.loadTasks();
  }

  loadPerformance(): void {
    this.performanceLoading = true;
    this.warningPage = 1;
    this.latePage = 1;
    this.employeeService.getPerformance(this.employeeId, this.dateRangeParams()).subscribe({
      next: (res) => { this.performance = res.data; this.performanceLoading = false; },
      error: () => { this.performanceLoading = false; }
    });
  }

  loadAccess(): void {
    if (this.access) return; // already loaded once this session
    this.accessLoading = true;
    this.employeeService.getAccess(this.employeeId).subscribe({
      next: (res) => { this.access = res.data; this.accessLoading = false; },
      error: () => { this.accessLoading = false; }
    });
  }

  loadDiscounts(): void {
    this.discountsLoading = true;
    this.discountPage = 1;
    this.employeeService.get360Discounts(this.employeeId, this.dateRangeParams()).subscribe({
      next: (res) => {
        this.discountsData = res.data?.discounts ?? [];
        this.discountsTotalAmount = res.data?.totalAmount ?? 0;
        this.discountsLoading = false;
      },
      error: () => { this.discountsLoading = false; }
    });
  }

  loadLeave(): void {
    this.leaveLoading = true;
    this.leavePage = 1;
    this.employeeService.getLeave360(this.employeeId, this.dateRangeParams()).subscribe({
      next: (res) => { this.leave = res.data; this.leaveLoading = false; },
      error: () => { this.leaveLoading = false; }
    });
  }

  isLeavePending(status: string): boolean {
    return (status || '').toLowerCase() === 'pending';
  }

  approveLeave(leaveId: number): void {
    if (!this.canApproveLeave) {
      this.toastr.error(this.translate.instant('FORBIDDEN.MESSAGE'));
      return;
    }

    Swal.fire({
      title: this.translate.instant('LEAVE.APPROVE_CONFIRM'),
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: this.translate.instant('LEAVE.APPROVE'),
      cancelButtonText: this.translate.instant('COMMON.CANCEL')
    }).then(result => {
      if (!result.isConfirmed) return;
      this.leaveService.approve(leaveId).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('LEAVE.APPROVED_SUCCESS'));
          this.loadLeave();
          this.load360(false);
        },
        error: () => this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'))
      });
    });
  }

  rejectLeave(leaveId: number): void {
    if (!this.canRejectLeave) {
      this.toastr.error(this.translate.instant('FORBIDDEN.MESSAGE'));
      return;
    }

    Swal.fire({
      title: this.translate.instant('LEAVE.REJECT'),
      input: 'textarea',
      inputLabel: this.translate.instant('LEAVE.REJECT_REASON'),
      inputPlaceholder: this.translate.instant('LEAVE.REJECT_REASON'),
      showCancelButton: true,
      confirmButtonText: this.translate.instant('LEAVE.REJECT'),
      cancelButtonText: this.translate.instant('COMMON.CANCEL'),
      inputValidator: (value) => {
        if (!value?.trim()) {
          return this.translate.instant('LEAVE.REJECT_REASON_REQUIRED');
        }
        return null;
      }
    }).then(result => {
      if (!result.isConfirmed) return;
      this.leaveService.reject(leaveId, String(result.value).trim()).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('LEAVE.REJECTED_SUCCESS'));
          this.loadLeave();
          this.load360(false);
        },
        error: () => this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'))
      });
    });
  }

  loadEmails(): void {
    this.emailLoading = true;
    this.employeeService.getEmails(this.employeeId, {
      pageIndex: this.emailPage,
      pageSize: this.emailPageSize,
      searchKey: this.emailSearch || undefined,
      ...this.dateRangeParams(),
      status: this.emailStatus || undefined,
      type: this.emailType || undefined
    }).subscribe({
      next: (res) => {
        this.emails = res.data?.data ?? [];
        this.emailTotal = res.data?.totalCount ?? 0;
        this.emailLoading = false;
      },
      error: () => { this.emailLoading = false; }
    });
  }

  applyEmailFilters(): void {
    this.emailPage = 1;
    this.loadEmails();
  }

  loadNotifications(): void {
    this.notifLoading = true;
    this.employeeService.getNotifications360(this.employeeId, {
      pageIndex: this.notifPage,
      pageSize: this.notifPageSize,
      searchKey: this.notifSearch || undefined,
      ...this.dateRangeParams(),
      status: this.notifStatus || undefined,
      channel: this.notifChannel || undefined,
      unreadOnly: this.notifUnreadOnly || undefined
    }).subscribe({
      next: (res) => {
        this.notifications = res.data?.data ?? [];
        this.notifTotal = res.data?.totalCount ?? 0;
        this.notifLoading = false;
      },
      error: () => { this.notifLoading = false; }
    });
  }

  applyNotifFilters(): void {
    this.notifPage = 1;
    this.loadNotifications();
  }

  loadComments(): void {
    this.commentLoading = true;
    this.openAttachmentsForCommentId = null;
    this.employeeService.getComments360(this.employeeId, {
      pageIndex: this.commentPage,
      pageSize: this.commentPageSize,
      searchKey: this.commentSearch || undefined,
      ...this.dateRangeParams()
    }).subscribe({
      next: (res) => {
        this.comments = res.data?.data ?? [];
        this.commentTotal = res.data?.totalCount ?? 0;
        this.commentLoading = false;
      },
      error: () => { this.commentLoading = false; }
    });
  }

  applyCommentFilters(): void {
    this.commentPage = 1;
    this.loadComments();
  }

  toggleCommentAttachments(event: Event, commentId: number): void {
    event.preventDefault();
    event.stopPropagation();

    if (this.openAttachmentsForCommentId === commentId) {
      this.openAttachmentsForCommentId = null;
      return;
    }

    this.openAttachmentsForCommentId = commentId;
    if (this.attachmentsMap[commentId]) return;

    this.loadingAttachments[commentId] = true;
    this.commentService.getCommentAttachments(commentId).subscribe({
      next: (res) => {
        this.attachmentsMap[commentId] = (res?.data ?? []) as AttachmentVm[];
        this.loadingAttachments[commentId] = false;
      },
      error: () => {
        this.attachmentsMap[commentId] = [];
        this.loadingAttachments[commentId] = false;
      }
    });
  }

  openCommentAttachment(event: Event, att: AttachmentVm): void {
    event.preventDefault();
    event.stopPropagation();
    if (att.url) {
      window.open(att.url, '_blank');
      return;
    }
    this.commentService.downloadAttachment(att.id).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        window.open(url, '_blank');
        setTimeout(() => URL.revokeObjectURL(url), 60_000);
      }
    });
  }

  loadTimeline(): void {
    this.timelineLoading = true;
    this.employeeService.getTimeline(this.employeeId, {
      pageIndex: this.timelinePage,
      pageSize: this.timelinePageSize,
      ...this.dateRangeParams(),
      searchKey: this.timelineSearch || undefined
    }).subscribe({
      next: (res) => {
        this.timeline = res.data?.data ?? [];
        this.timelineTotal = res.data?.totalCount ?? 0;
        this.timelineLoading = false;
      },
      error: () => { this.timelineLoading = false; }
    });
  }

  applyTimelineFilters(): void {
    this.timelinePage = 1;
    this.loadTimeline();
  }

  onTimelinePageChange(page: number): void {
    this.timelinePage = page;
    this.loadTimeline();
  }

  openCreateTask(modal: any): void {
    this.modalKey++;
    this.modalService.open(modal, {
      centered: true,
      backdrop: 'static',
      keyboard: false,
      size: 'xl'
    });
  }

  onTaskCreated(): void {
    this.modalService.dismissAll();
    this.load360();
    if (this.activeTab === 'work') this.loadTasks();
  }

  sendEmail(): void {
    const email = this.profile?.email;
    if (!email) {
      this.toastr.warning(this.translate.instant('EMPLOYEE_360.NO_EMAIL'));
      return;
    }
    window.location.href = `mailto:${email}`;
  }

  goToEdit(modal: any): void {
    this.employeeEditKey++;
    this.modalService.open(modal, {
      centered: true,
      backdrop: 'static',
      keyboard: false,
      size: 'xl',
      windowClass: 'effect-scale'
    });
  }

  onEmployeeUpdated(): void {
    this.modalService.dismissAll();
    this.load360();
  }

  timelineIcon(type: string): string {
    const map: Record<string, string> = {
      TaskAssigned: 'bi-list-task',
      TaskCompleted: 'bi-check2-circle',
      Comment: 'bi-chat-left-text',
      Warning: 'bi-exclamation-triangle',
      Discount: 'bi-cash-coin',
      Progress: 'bi-graph-up',
      LeaveSubmitted: 'bi-calendar-plus',
      LeaveApproved: 'bi-calendar-check',
      LeaveRejected: 'bi-calendar-x',
      EmailSent: 'bi-envelope',
      NotificationSent: 'bi-bell'
    };
    return map[type] || 'bi-circle';
  }

  statusBadgeClass(status: string): string {
    const s = (status || '').toLowerCase();
    if (['sent', 'delivered', 'read', 'approved', 'opened', 'clicked'].includes(s)) return 'bg-success';
    if (['queued', 'processing', 'pending'].includes(s)) return 'bg-warning text-dark';
    if (['failed', 'bounced', 'rejected', 'spam'].includes(s)) return 'bg-danger';
    return 'bg-secondary';
  }

  translateEmailType(type?: string | null): string {
    return this.translateLookup('EMPLOYEE_360.EMAIL_TYPES', type);
  }

  translateNotificationType(type?: string | null): string {
    return this.translateLookup('EMPLOYEE_360.NOTIFICATION_TYPES', type);
  }

  private translateLookup(prefix: string, key?: string | null): string {
    if (!key) return '—';
    const fullKey = `${prefix}.${key}`;
    const translated = this.translate.instant(fullKey);
    return translated !== fullKey ? translated : key;
  }

  get profile() { return this.data?.profile; }
  get kpis() { return this.data?.kpis; }
  get sidebar() { return this.data?.sidebar; }

  get filteredWarnings(): EmployeeWarningDto[] {
    return (this.performance?.warnings ?? []).filter(w =>
      this.matchesSearch(this.performanceSearch, [
        w.reason, w.taskTitle, w.taskStatus, w.taskId, w.id, w.createdDate
      ])
    );
  }

  get pagedWarnings(): EmployeeWarningDto[] {
    return this.paginate(this.filteredWarnings, this.warningPage);
  }

  get filteredLateTasks(): Employee360Deadline[] {
    return (this.performance?.lateTasks ?? []).filter(t =>
      this.matchesSearch(this.performanceSearch, [
        t.title, t.status, t.priority, t.taskId, t.dueDate
      ])
    );
  }

  get pagedLateTasks(): Employee360Deadline[] {
    return this.paginate(this.filteredLateTasks, this.latePage);
  }

  get filteredDiscounts(): EmployeeDeductionDto[] {
    return this.discountsData.filter(d =>
      this.matchesSearch(this.discountSearch, [
        d.reason, d.taskTitle, d.amount, d.taskId, d.id, d.createdDate,
        d.autoDiscount ? 'Auto' : 'Manual',
        d.autoDiscount ? 'تلقائي' : 'يدوي',
        d.discountType
      ])
    );
  }

  get pagedDiscounts(): EmployeeDeductionDto[] {
    return this.paginate(this.filteredDiscounts, this.discountPage);
  }

  get filteredLeaveHistory() {
    return (this.leave?.history ?? []).filter(l =>
      this.matchesSearch(this.leaveSearch, [
        l.leaveTypeName, l.status, l.notes, l.startDate, l.endDate, l.id, l.createdDate
      ])
    );
  }

  get pagedLeaveHistory() {
    return this.paginate(this.filteredLeaveHistory, this.leavePage);
  }

  get filteredAccessRoles() {
    const roles = this.access?.roles ?? [];
    if (!this.accessSearch.trim()) return roles;
    return roles.filter(r =>
      this.matchesSearch(this.accessSearch, [r.name, r.roleId, r.level])
    );
  }

  get filteredAccessPermissions() {
    const permissions = this.access?.permissions ?? [];
    if (!this.accessSearch.trim()) return permissions;
    return permissions.filter(p => this.matchesSearch(this.accessSearch, [p]));
  }

  get filteredAccessGroups() {
    const groups = this.access?.permissionGroups ?? [];
    if (!this.accessSearch.trim()) return groups;
    return groups.filter(g => this.matchesSearch(this.accessSearch, [g]));
  }

  get discountsWithTaskCount(): number {
    const ids = new Set(
      this.filteredDiscounts.filter(d => !!d.taskId).map(d => d.taskId!)
    );
    return ids.size;
  }

  onPerformanceSearchChange(): void {
    this.warningPage = 1;
    this.latePage = 1;
  }

  onDiscountSearchChange(): void {
    this.discountPage = 1;
  }

  onLeaveSearchChange(): void {
    this.leavePage = 1;
  }

  private paginate<T>(items: T[], page: number): T[] {
    const size = this.listPageSize;
    const maxPage = Math.max(1, Math.ceil(items.length / size) || 1);
    const safePage = Math.min(Math.max(1, page), maxPage);
    const start = (safePage - 1) * size;
    return items.slice(start, start + size);
  }

  private matchesSearch(search: string, values: Array<string | number | null | undefined>): boolean {
    const key = (search || '').trim().toLowerCase();
    if (!key) return true;
    return values.some(v => v != null && String(v).toLowerCase().includes(key));
  }
}
