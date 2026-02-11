import { Component } from '@angular/core';
import { SharedModule } from '../../../shared/shared.module';
import * as chartData from '../../../shared/data/dashboard';
import { NgbModal, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgCircleProgressModule } from 'ng-circle-progress';
import { NgApexchartsModule } from 'ng-apexcharts';
import { SpkDashboardComponent } from '../../../@spk/reusable-dashboard/spk-dashboard/spk-dashboard.component';
import { SpkReusableTablesComponent } from '../../../@spk/reusable-tables/spk-reusable-tables/spk-reusable-tables.component';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { DashboardService } from 'app/core/services/dashboar.service';
import {
  AdminDashboardDto,
  AdminKpisExtendedDto,
  BranchFilterDto,
  CompletedTasksTodayDto,
  EmployeeDashboardDto,
  EmployeeKpisExtendedDto,
  InProgressUpdatedTodayDto,
  PendingCloseRequestDto,
  WarningDto,
} from 'app/core/models/dashboard/dashboard.model';
import { AuthService } from 'app/core/services/auth.service';
import { TaskStatusPopupComponent } from '../dashboard-pop-ups/task-status-popup.component';
import { PeriodDto, PeriodType } from 'app/models/period-type.model';
import { FormsModule } from '@angular/forms';
import { HighPriorityTasksPopupComponent } from '../dashboard-pop-ups/high-priority-open-tasks-popup';
import { CompletedTasksPopupComponent } from '../dashboard-pop-ups/average-completion-hours-popup';
import { DiscountsPopupComponent } from '../dashboard-pop-ups/penalities-details-popup';
import { EmployeeTasksPopupComponent } from '../employee-dashboard-pop-ups/active-tasks-popup';
import { WarningsPopupComponent } from '../employee-dashboard-pop-ups/my-warning-popup';
import { TaskTodayCommentComponent } from '../task-today-comment/task-today-comment.component';
import { PenalitiesPopupComponent } from '../employee-dashboard-pop-ups/my-penalities-popup';
import { MyDueSoonTasksPopupComponent } from '../employee-dashboard-pop-ups/due-soon-tasks';
import { TaskDetailsShellComponent } from 'app/components/tasks/task-details/task-details-shell/task-details-shell.component';
import { TaskTodayUpdatedComponent } from '../task-today-updated/task-today-updated.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    SharedModule,
    NgbModule,
    NgSelectModule,
    NgCircleProgressModule,
    NgApexchartsModule,
    SpkDashboardComponent,
    SpkReusableTablesComponent,
    CommonModule,
    TranslateModule,
    FormsModule,
    TaskTodayCommentComponent,
    TaskTodayUpdatedComponent
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  isAdmin = false;

  adminData!: AdminDashboardDto;
  adminKpis!: AdminKpisExtendedDto;

  employeeData!: EmployeeDashboardDto;
  EmployeeKpis!: EmployeeKpisExtendedDto;

  myWarning!: WarningDto;

  topDelayedEmployees: any[] = [];
  todayInProgressTasks: InProgressUpdatedTodayDto[] = [];
  completedTasksToday: CompletedTasksTodayDto[] = [];
  pendingCloseRequests: PendingCloseRequestDto[] = [];

  periodOptions = [
    { label: 'DASHBOARD.TODAY', value: 'Day' },
    { label: 'DASHBOARD.MONTH', value: 'Month' },
    { label: 'DASHBOARD.YEAR', value: 'Year' },
  ];

  selectedPeriod: PeriodType = 'Day';
  currentPeriod: PeriodDto = { type: 'Day' };
  currentBranchId: number | null = null;

  branches: BranchFilterDto[] = [];
  selectedBranchId?: number;

  kpiCards: any[] = [];
  statCards: any[] = [];
  tasks: any[] = [];

  // charts
  public ChartOptions = chartData.ChartOptions;
  public ChartOptions1 = chartData.ChartOptions1;
  public ChartOptions2 = chartData.ChartOptions2;
  public barChartOptions = chartData.barChartOptions;
  public barChartType = chartData.barChartType;
  public barChartData = chartData.barChartData;

  transactions = [
    {
      avatar: './assets/images/faces/5.jpg',
      name: 'Flicker',
      description: 'App improvement',
      amount: '$45.234',
      date: '12 Jan 2020',
      trendClass: 'up-alt text-success',
    },
    {
      avatar: './assets/images/faces/6.jpg',
      name: 'Intoxica',
      description: 'Milestone',
      amount: '$23.452',
      date: '23 Jan 2020',
      trend: 'down',
      trendClass: 'down-alt text-danger',
    },
    {
      avatar: './assets/images/faces/7.jpg',
      name: 'Digiwatt',
      description: 'Sales executive',
      amount: '$78.001',
      date: '4 Apr 2020',
      trend: 'down',
      trendClass: 'down-alt text-danger',
    },
    {
      avatar: './assets/images/faces/8.jpg',
      name: 'Flicker',
      description: 'Milestone2',
      amount: '$37.285',
      date: '4 Apr 2020',
      trendClass: 'up-alt text-success',
    },
    {
      avatar: './assets/images/faces/4.jpg',
      name: 'Flicker',
      description: 'App improvement',
      amount: '$25.341',
      date: '4 Apr 2020',
      trendClass: 'down-alt text-danger',
      cell: 'pb-0',
    },
  ];

  taskColumns = [
    { header: 'Task', field: 'Task', tableHeadColumn: 'wd-lg-20p' },
    { header: 'Team', field: 'Team', tableHeadColumn: 'wd-lg-20p text-center' },
    { header: 'Open task', field: 'Task', tableHeadColumn: 'wd-lg-20p text-center' },
    { header: 'Priority', field: 'Priority', tableHeadColumn: 'wd-lg-20p' },
    { header: 'Status', field: 'Status', tableHeadColumn: 'wd-lg-20p' },
  ];

  constructor(
    private translate: TranslateService,
    private dashboardService: DashboardService,
    private authService: AuthService,
    private modalService: NgbModal
  ) {
    this.translate.use('ar');
  }

  ngOnInit(): void {
    this.periodOptions = [
      { label: this.translate.instant('DASHBOARD.TODAY'), value: 'Day' },
      { label: this.translate.instant('DASHBOARD.MONTH'), value: 'Month' },
      { label: this.translate.instant('DASHBOARD.YEAR'), value: 'Year' },
    ];

    this.translate.use('ar');
    this.loadDashboard();
  }

  private loadDashboard() {
    const userLevel = this.authService.getRoleLevel() ?? 0;

    if (userLevel >= 50) {
      this.isAdmin = true;

      // branches مرة واحدة
      this.loadBranchesForFilter();

      // أول تحميل
      this.reloadDashboard();
    } else {
      this.isAdmin = false;
      this.loadEmployeeDashboard();
      this.loadEmployeeKpis();
    }
  }

  onPeriodChanged() {
    this.currentPeriod = { type: this.selectedPeriod };
    this.reloadDashboard();
  }

  onBranchChanged() {
    this.reloadDashboard();
  }

  private reloadDashboard() {
    if (this.isAdmin) {
      this.loadAdminKpis();
      this.loadAdminDashboard();
      this.loadCompletedTasksToday();
      this.loadPendingCloseRequests();
      // لو عندك شاشة today updated هتضيفيها هنا مع branchId
      // this.loadTodayInProgressTasks();
    } else {
      this.loadEmployeeDashboard();
      this.loadEmployeeKpis();
    }
  }

  // ======================= Admin loads =======================

  private loadAdminDashboard() {
    this.dashboardService
      .getAdminDashboard(this.currentPeriod, this.selectedBranchId)
      .subscribe((res) => {
        this.adminData = res.data;
        this.topDelayedEmployees = res.data?.topDelayedEmployees ?? [];

        this.statCards = [
          {
            title: 'DASHBOARD.OVERDUE_TASKS',
            value: this.adminData.kpis.overdueTasks,
            svg: `
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" fill="none"
                   stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                   class="feather feather-alert-circle text-danger">
                <circle cx="12" cy="12" r="10"></circle>
                <line x1="12" y1="8" x2="12" y2="12"></line>
                <line x1="12" y1="16" x2="12.01" y2="16"></line>
              </svg>
            `,
            clickable: true,
            status: 'Overdue',
          },
          {
            title: 'DASHBOARD.ACTIVE_TASKS',
            value: this.adminData.kpis.activeTasks,
            svg: `
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
                   viewBox="0 0 24 24" fill="none" stroke="currentColor"
                   stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                   class="feather feather-activity text-warning">
                <polyline points="22 12 18 12 15 21 9 3 6 12 2 12"></polyline>
              </svg>
            `,
            clickable: true,
            status: 'Active',
          },
          {
            title: 'DASHBOARD.TOTAL_EMPLOYEES',
            value: this.adminData.kpis.totalEmployees,
            svg: `
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
                   viewBox="0 0 24 24" fill="none" stroke="currentColor"
                   stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                   class="feather feather-users text-primary">
                <path d="M17 21v-2a4 4 0 0 0-3-3.87"></path>
                <path d="M7 21v-2a4 4 0 0 1 3-3.87"></path>
                <circle cx="12" cy="7" r="4"></circle>
              </svg>
            `,
          },
          {
            title: 'DASHBOARD.COMPLETED_TASKS',
            value: this.adminData.kpis.completedTasks,
            svg: `
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
                   viewBox="0 0 24 24" fill="none" stroke="currentColor"
                   stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                   class="feather feather-check-circle text-success">
                <path d="M9 12l2 2 4-4"></path>
                <circle cx="12" cy="12" r="10"></circle>
              </svg>
            `,
            clickable: true,
            status: 'Completed',
          },
        ];
      });
  }

  private loadAdminKpis() {
    this.dashboardService
      .getAdminKpis(this.currentPeriod, this.selectedBranchId)
      .subscribe((res) => {
        this.adminKpis = res.data;
        this.buildAdminKpiCards();
      });
  }

  private loadCompletedTasksToday() {
    this.dashboardService
      .getAdminCompletedTasksToday(this.currentPeriod, this.selectedBranchId)
      .subscribe((res) => {
        this.completedTasksToday = res.data ?? [];
      });
  }

  private loadPendingCloseRequests() {
    this.dashboardService
      .getAdminPendingCloseRequests(this.currentPeriod, this.selectedBranchId)
      .subscribe((res) => {
        this.pendingCloseRequests = res.data ?? [];
      });
  }

  private buildAdminKpiCards() {
    this.kpiCards = [
      {
        title: 'DASHBOARD.AVG_COMPLETION_TIME',
        value: (this.adminKpis.averageCompletionHours / 24).toFixed(1) + this.translate.instant('TASK.DAYS'),
        svg: `
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
               viewBox="0 0 24 24" fill="none" stroke="currentColor"
               stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
               class="feather feather-clock text-primary">
            <circle cx="12" cy="12" r="10"></circle>
            <polyline points="12 6 12 12 16 14"></polyline>
          </svg>
        `,
        clickable: true,
        type: 'avgCompletion'
      },
      {
        title: 'DASHBOARD.ON_TIME_RATE',
        value: this.adminKpis.onTimeRatePercent.toFixed(0) + '%',
        svg: `
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
               viewBox="0 0 24 24" fill="none" stroke="currentColor"
               stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
               class="feather feather-trending-up text-success">
            <polyline points="23 6 13.5 15.5 8.5 10.5 1 18"></polyline>
            <polyline points="17 6 23 6 23 12"></polyline>
          </svg>
        `,
      },
      {
        title: 'DASHBOARD.HIGH_PRIORITY',
        value: this.adminKpis.highPriorityOpenTasks,
        svg: `
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
               viewBox="0 0 24 24" fill="none" stroke="currentColor"
               stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
               class="feather feather-flag text-danger">
            <path d="M4 15s1-1 4-1 5 2 8 2 4-1 4-1V3s-1 1-4 1-5-2-8-2-4 1-4 1z"></path>
            <line x1="4" y1="22" x2="4" y2="15"></line>
          </svg>
        `,
        clickable: true,
        type: 'highPriority'
      },
      {
        title: 'DASHBOARD.PENALTIES',
        value: this.adminKpis.penaltiesThisMonth + this.translate.instant('TASK.SAR'),
        svg: `
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
               viewBox="0 0 24 24" fill="none" stroke="currentColor"
               stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
               class="feather feather-dollar-sign text-danger">
            <line x1="12" y1="1" x2="12" y2="23"></line>
            <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"></path>
          </svg>
        `,
        clickable: true,
        type: 'penalties'
      },
    ];
  }

  loadBranchesForFilter() {
    this.dashboardService.getBranchesForFilter().subscribe({
      next: (res) => {
        this.branches = res.data ?? [];

        // لو رجع فرع واحد (Branch Manager) اختاريه واعيدي تحميل الداتا
        if (this.branches.length === 1) {
          const newId = this.branches[0].id;
          const changed = this.selectedBranchId !== newId;
          this.selectedBranchId = newId;

          if (changed) {
            this.reloadDashboard();
          }
        }
      }
    });
  }

  // ======================= Employee loads =======================

  private loadEmployeeDashboard(): void {
    this.dashboardService.getEmployeeDashboard(this.currentPeriod).subscribe((res) => {
      this.employeeData = res.data;

      this.statCards = [
        {
          title: 'DASHBOARD.MY_ACTIVE_TASKS',
          value: this.employeeData.kpis.myActiveTasks,
          svg: `
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
                 fill="none" stroke="currentColor" stroke-width="2"
                 stroke-linecap="round" stroke-linejoin="round"
                 class="feather feather-list text-primary">
              <line x1="8" y1="6" x2="21" y2="6"></line>
              <line x1="8" y1="12" x2="21" y2="12"></line>
              <line x1="8" y1="18" x2="21" y2="18"></line>
              <line x1="3" y1="6" x2="3.01" y2="6"></line>
              <line x1="3" y1="12" x2="3.01" y2="12"></line>
              <line x1="3" y1="18" x2="3.01" y2="18"></line>
            </svg>
          `,
          clickable: true,
          type: 'activeTasks'
        },
        {
          title: 'DASHBOARD.DUE_SOON',
          value: this.employeeData.kpis.dueSoonTasks,
          svg: `
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
                 fill="none" stroke="currentColor" stroke-width="2"
                 stroke-linecap="round" stroke-linejoin="round"
                 class="feather feather-clock text-warning">
              <circle cx="12" cy="12" r="10"></circle>
              <polyline points="12 6 12 12 16 14"></polyline>
            </svg>
          `,
          clickable: true,
          type: 'myDueSoonTasks'
        },
        {
          title: 'DASHBOARD.MY_WARNINGS',
          value: this.employeeData.kpis.myWarnings,
          svg: `
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
                 fill="none" stroke="currentColor" stroke-width="2"
                 stroke-linecap="round" stroke-linejoin="round"
                 class="feather feather-alert-triangle text-danger">
              <path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path>
              <line x1="12" y1="9" x2="12" y2="13"></line>
              <line x1="12" y1="17" x2="12.01" y2="17"></line>
            </svg>
          `,
          clickable: true,
          type: 'myWarnings'
        },
        {
          title: 'DASHBOARD.PENALTIES',
          value: this.employeeData.kpis.myPenalties + ' ' + this.translate.instant('TASK.SAR'),
          svg: `
            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
                 viewBox="0 0 24 24" fill="none" stroke="currentColor"
                 stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                 class="feather feather-dollar-sign text-danger">
              <line x1="12" y1="1" x2="12" y2="23"></line>
              <path d="M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"></path>
            </svg>
          `,
          clickable: true,
          type: 'myPenalities'
        },
      ];

      this.tasks = this.employeeData.myTasks.map((t) => ({
        name: t.title,
        checked: t.status === 'Closed',
        status: t.status,
        comments: `${t.progressPercent}%`,
      }));
    });
  }

  private loadEmployeeKpis() {
    this.dashboardService.getEmployeeKpis(this.currentPeriod).subscribe((res) => {
      this.EmployeeKpis = res.data;
      this.buildEmployeeKpiCards();
    });
  }

  private buildEmployeeKpiCards() {
    this.kpiCards = [
      {
        title: 'DASHBOARD.AVG_COMPLETION_TIME',
        value: (this.EmployeeKpis.averageCompletionHours / 24).toFixed(1) + this.translate.instant('TASK.DAYS'),
        svg: `
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
               viewBox="0 0 24 24" fill="none" stroke="currentColor"
               stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
               class="feather feather-clock text-primary">
            <circle cx="12" cy="12" r="10"></circle>
            <polyline points="12 6 12 12 16 14"></polyline>
          </svg>
        `,
        clickable: true,
        type: 'empAvgCompletion'
      },
      {
        title: 'DASHBOARD.ON_TIME_RATE',
        value: this.EmployeeKpis.onTimeRatePercent.toFixed(0) + '%',
        svg: `
          <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
               viewBox="0 0 24 24" fill="none" stroke="currentColor"
               stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
               class="feather feather-trending-up text-success">
            <polyline points="23 6 13.5 15.5 8.5 10.5 1 18"></polyline>
            <polyline points="17 6 23 6 23 12"></polyline>
          </svg>
        `,
      }
    ];
  }

  // ======================= UI helpers =======================

  checkRowClickable(task: InProgressUpdatedTodayDto): boolean {
    return !!task?.taskId;
  }

  openTaskDetails(taskId: number): void {
    if (!taskId) return;

    const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      backdrop: 'static',
      scrollable: true
    });

    modalRef.componentInstance.taskId = taskId;
    modalRef.componentInstance.readonly = true;
  }

  // ======================= Cards / Popups =======================

  openCard(card: any) {
    if (!card.clickable) return;

    if (card.status) {
      this.dashboardService
        .getAdminTasksByStatus(card.status, this.currentPeriod, this.selectedBranchId)
        .subscribe((res) => {
          const modalRef = this.modalService.open(TaskStatusPopupComponent, {
            size: 'lg',
            centered: true,
          });
          modalRef.componentInstance.tasks = res.data;
        });

      return;
    }

    if (card.type === 'avgCompletion') this.openAvgCompletionTasks();
    else if (card.type === 'highPriority') this.openHighPriorityTasks();
    else if (card.type === 'penalties') this.openDiscounts();
    else if (card.type === 'activeTasks') this.openEmployeeTasksPopup();
    else if (card.type === 'myWarnings') this.openMyWarning();
    else if (card.type === 'myPenalities') this.openMyPenalities();
    else if (card.type === 'myDueSoonTasks') this.openEmployeeDueSoonTasksPopup();
    else if (card.type === 'empAvgCompletion') this.openEmployeeAvgCompletionTasks();
  }

  openAvgCompletionTasks() {
    this.dashboardService
      .getAdminCompletedTasksDetails(this.currentPeriod, this.selectedBranchId)
      .subscribe(res => {
        const modalRef = this.modalService.open(CompletedTasksPopupComponent, {
          size: 'xl',
          centered: true
        });
        modalRef.componentInstance.tasks = res.data;
      });
  }

  openEmployeeAvgCompletionTasks() {
    this.dashboardService.getEmployeeCompletedTasksDetails(this.currentPeriod)
      .subscribe(res => {
        const modalRef = this.modalService.open(CompletedTasksPopupComponent, {
          size: 'xl',
          centered: true
        });
        modalRef.componentInstance.tasks = res.data;
      });
  }

  openHighPriorityTasks() {
    this.dashboardService.getAdminHighPriorityTasks(this.selectedBranchId)
      .subscribe(res => {
              console.log('HighPriority sample:', res.data?.[0]);
        const modalRef = this.modalService.open(HighPriorityTasksPopupComponent, {
          size: 'xl',
          centered: true
        });
        modalRef.componentInstance.tasks = res.data;
      });
  }

  openDiscounts() {
    this.dashboardService.getAdminDiscounts(this.currentPeriod, this.selectedBranchId)
      .subscribe(res => {
        const modalRef = this.modalService.open(DiscountsPopupComponent, {
          size: 'xl',
          centered: true
        });
        modalRef.componentInstance.discounts = res.data;
      });
  }

  openEmployeeTasksPopup() {
    if (!this.employeeData || !this.employeeData.myTasks) return;

    const modalRef = this.modalService.open(EmployeeTasksPopupComponent, {
      size: 'lg',
      centered: true
    });

    modalRef.componentInstance.tasks = this.employeeData.myTasks;
  }

  openEmployeeDueSoonTasksPopup() {
    this.dashboardService.getMyDueSoonTask()
      .subscribe(res => {
        const modalRef = this.modalService.open(MyDueSoonTasksPopupComponent, {
          size: 'lg',
          centered: true
        });
        modalRef.componentInstance.tasks = res.data ?? [];
      });
  }

  openMyWarning() {
    this.dashboardService.getEmployeeWarnings(this.currentPeriod)
      .subscribe(res => {
        const modalRef = this.modalService.open(WarningsPopupComponent, {
          size: 'lg',
          centered: true
        });
        modalRef.componentInstance.warnings = res.data ?? [];
      });
  }

  openMyPenalities() {
    this.dashboardService.getEmployeePenalities(this.currentPeriod)
      .subscribe(res => {
        const modalRef = this.modalService.open(PenalitiesPopupComponent, {
          size: 'lg',
          centered: true
        });
        modalRef.componentInstance.Penalities = res.data ?? [];
      });
  }
}
