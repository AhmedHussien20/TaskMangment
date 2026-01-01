import { Component } from '@angular/core';
import { SharedModule } from '../../../shared/shared.module';
import * as chartData from '../../../shared/data/dashboard';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgSelectModule } from '@ng-select/ng-select';
import { NgCircleProgressModule } from 'ng-circle-progress';
import { NgApexchartsModule } from 'ng-apexcharts';
import { SpkApexChartsComponent } from '../../../@spk/reusable-charts/spk-apex-charts/spk-apex-charts.component';
import { SpkDashboardComponent } from '../../../@spk/reusable-dashboard/spk-dashboard/spk-dashboard.component';
import { SpkReusableTablesComponent } from '../../../@spk/reusable-tables/spk-reusable-tables/spk-reusable-tables.component';
import { CommonModule } from '@angular/common';
import {
  TranslateModule,
  TranslateService,
  TranslateStore,
} from '@ngx-translate/core';
import { DashboardService } from 'app/core/services/dashboar.service';
import { AdminDashboardDto, CompletedTasksTodayDto, EmployeeDashboardDto, InProgressUpdatedTodayDto, PendingCloseRequestDto } from 'app/core/models/dashboard/dashboard.model';
import { AuthService } from 'app/core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    SharedModule,
    NgbModule,
    NgSelectModule,
    NgCircleProgressModule,
    NgApexchartsModule,
    SpkApexChartsComponent,
    SpkDashboardComponent,
    SpkReusableTablesComponent,
    CommonModule,
    TranslateModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.scss',
})
export class DashboardComponent {
  isAdmin = false;
  adminData!: AdminDashboardDto;
  employeeData!: EmployeeDashboardDto;
  topDelayedEmployees: any[] = [];
  todayInProgressTasks: InProgressUpdatedTodayDto[] = [];
  completedTasksToday: CompletedTasksTodayDto[] = [];
  pendingCloseRequests: PendingCloseRequestDto[] = [];

  cards: any[] = [];
  tasks: any[] = [];
  constructor(private translate: TranslateService, private dashboardService: DashboardService, private authService: AuthService
  ) {
    console.log('DashboardComponent');
    this.translate.use('ar');
  }

  private loadDashboard() {
    const user = this.authService.getCurrentUser();
    const userLevel = this.authService.getRoleLevel() ?? 0;
    if (userLevel >= 50) {
      this.loadAdminDashboard();
      this.loadTodayInProgressTasks();
      this.loadCompletedTasksToday();
      this.loadPendingCloseRequests();
    } else {
      this.loadEmployeeDashboard();
    }
  }

  private loadCompletedTasksToday() {
    this.dashboardService
      .getAdminCompletedTasksToday()
      .subscribe(res => {
        this.completedTasksToday = res.data ?? [];
      });
  }

  private loadPendingCloseRequests() {
    this.dashboardService
      .getAdminPendingCloseRequests()
      .subscribe(res => {
        this.pendingCloseRequests = res.data ?? [];
      });
  }

  private loadTodayInProgressTasks() {
    this.dashboardService
      .getAdminInProgressUpdatedToday()
      .subscribe(res => {
        this.todayInProgressTasks = res.data ?? [];
      });
  }

  private loadAdminDashboard() {
    this.dashboardService.getAdminDashboard().subscribe(res => {
      this.adminData = res.data;
      this.topDelayedEmployees = res.data.topDelayedEmployees ?? [];
      this.cards = [
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
    `
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
    `
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
    `
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
    `
        }
      ];

      // Chart
      //this.ChartOptions.series = this.adminData.taskStatusChart.items.map(x => x.count);
      //this.ChartOptions.labels = this.adminData.taskStatusChart.items.map(x => x.status);
    });
  }
 private loadEmployeeDashboard(): void {
  this.dashboardService.getEmployeeDashboard().subscribe(res => {
    this.employeeData = res.data;

    this.cards = [
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
        `
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
        `
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
        `
      }
    ];

    this.tasks = this.employeeData.myTasks.map(t => ({
      name: t.title,
      checked: t.status === 'Closed',
      status: t.status,
      comments: `${t.progressPercent}%`
    }));
  });
}


  ngOnInit(): void { this.translate.use('ar'); this.loadDashboard(); }
  //line Chart
  public ChartOptions = chartData.ChartOptions;
  public ChartOptions1 = chartData.ChartOptions1;
  public ChartOptions2 = chartData.ChartOptions2;
  //Bar chart
  public barChartOptions = chartData.barChartOptions;
  public barChartType = chartData.barChartType;
  public barChartData = chartData.barChartData;

  // circleOptions = {
  //   series: [1854, 250],
  //   labels: ['Bitcoin', 'Ethereum'],
  //   chart: {
  //     height: 73,
  //     width: 50,
  //     type: 'donut',
  //   },
  //   dataLabels: {
  //     enabled: false,
  //   },

  //   legend: {
  //     show: false,
  //   },
  //   stroke: {
  //     show: true,
  //     curve: 'smooth',
  //     lineCap: 'round',
  //     colors: '#fff',
  //     width: 0,
  //     dashArray: 0,
  //   },
  //   plotOptions: {
  //     pie: {
  //       expandOnClick: false,
  //       donut: {
  //         size: '75%',
  //         background: 'transparent',
  //         labels: {
  //           show: false,
  //           name: {
  //             show: true,
  //             fontSize: '20px',
  //             color: '#495057',
  //             offsetY: -4,
  //           },
  //           value: {
  //             show: true,
  //             fontSize: '18px',
  //             color: undefined,
  //             offsetY: 8,
  //             formatter: function (val: string) {
  //               return val + '%';
  //             },
  //           },
  //           total: {
  //             show: true,
  //             showAlways: true,
  //             label: 'Total',
  //             fontSize: '22px',
  //             fontWeight: 600,
  //             color: '#495057',
  //           },
  //         },
  //       },
  //     },
  //   },
  //   colors: ['var(--primary-color)', 'rgba(var(--primary-rgb), 0.2)'],
  // };
  // circleOptions1 = {
  //   series: [1754, 544],
  //   labels: ['Bitcoin', 'Ethereum'],
  //   chart: {
  //     height: 73,
  //     width: 50,
  //     type: 'donut',
  //   },
  //   dataLabels: {
  //     enabled: false,
  //   },

  //   legend: {
  //     show: false,
  //   },
  //   stroke: {
  //     show: true,
  //     curve: 'smooth',
  //     lineCap: 'round',
  //     colors: '#fff',
  //     width: 0,
  //     dashArray: 0,
  //   },
  //   plotOptions: {
  //     pie: {
  //       expandOnClick: false,
  //       donut: {
  //         size: '75%',
  //         background: 'transparent',
  //         labels: {
  //           show: false,
  //           name: {
  //             show: true,
  //             fontSize: '20px',
  //             color: '#495057',
  //             offsetY: -4,
  //           },
  //           value: {
  //             show: true,
  //             fontSize: '18px',
  //             color: undefined,
  //             offsetY: 8,
  //             // formatter: function (val: string) {
  //             //   return val + "%"
  //             // }
  //           },
  //           total: {
  //             show: true,
  //             showAlways: true,
  //             label: 'Total',
  //             fontSize: '22px',
  //             fontWeight: 600,
  //             color: '#495057',
  //           },
  //         },
  //       },
  //     },
  //   },
  //   colors: ['var(--primary-color)', 'rgba(var(--primary-rgb), 0.2)'],
  // };

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
    {
      header: 'Open task',
      field: 'Task',
      tableHeadColumn: 'wd-lg-20p text-center',
    },
    { header: 'Priority', field: 'Priority', tableHeadColumn: 'wd-lg-20p' },
    { header: 'Status', field: 'Status', tableHeadColumn: 'wd-lg-20p' },
  ];

}
