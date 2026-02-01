import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { EmployeeService } from 'app/core/services/employee.service';
import { AuthService } from 'app/core/services/auth.service';
import { SimpleEmployee } from 'app/core/models/task/task';

import { ChartConfiguration, ChartType } from 'chart.js';
import { SpkChartjsComponent } from '../../../@spk/reusable-charts/spk-chartjs/spk-chartjs.component';
import { SpkEchartsComponent } from '../../../@spk/reusable-charts/spk-echarts/spk-echarts.component';
import type { EChartsOption } from 'echarts';
import { ChartService } from 'app/core/services/Chart.Service';
import { EmpTaskChartItem, TaskStatusCountDto } from 'app/core/models/chart';


@Component({
  selector: 'app-emp-task-chart',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    PageHeaderComponent,
    SpkChartjsComponent,  
    SpkEchartsComponent  
  ],
  templateUrl: './emp-task-chart.component.html',
  styleUrl: './emp-task-chart.component.scss'
})
export class EmpTaskChartComponent implements OnInit {

  title = 'nav.apps.charts.emp_statistics';
  activeitem = 'nav.apps.charts.emp_statistics';
  breadcrumbs = ['MENU.HOME','nav.apps.charts.title','nav.apps.charts.emp_statistics'];

  employees: SimpleEmployee[] = [];
  selectedEmployeeId?: number;

  fromDate!: string;
  toDate?: string;

  isAdmin = false;
  isLoading = false;

  public barChartType: ChartType = 'bar';
  public barChartOptions: ChartConfiguration['options'] = {
    maintainAspectRatio: false,
    responsive: true,
    plugins: {
      legend: { display: true, labels: { color: '#77778e' } }
    },
    scales: {
      y: {
        beginAtZero: true,
        ticks: { color: '#77778e' },
        grid: { color: 'rgba(119, 119, 142, 0.2)' }
      },
      x: {
        ticks: { color: '#77778e' },
        grid: { display: false }
      }
    }
  };

  public barChartData: ChartConfiguration['data'] = {
    labels: [],
    datasets: [{
      label: '',
      data: [],
      backgroundColor: [],
      borderColor: [],
      borderWidth: 1
    }]
  };

  public pieOptions: EChartsOption = this.buildPieOptions([], []);

  private readonly colors = [
    'rgba(98, 89, 202, 0.2)',
    'rgba(1, 184, 255, 0.2)',
    'rgba(255, 155, 33, 0.2)',
    'rgba(0, 204, 204, 0.2)',
    'rgba(253, 96, 116, 0.2)',
    'rgba(25, 177, 89, 0.2)',
    'rgba(35, 35, 35, 0.2)'
  ];

  private readonly borderColors = [
    'rgb(98, 89, 202)',
    'rgb(1, 184, 255)',
    'rgb(255, 155, 33)',
    'rgb(0, 204, 204)',
    'rgb(253, 96, 116)',
    'rgb(25, 177, 89)',
    'rgb(35, 35, 35)'
  ];

  private readonly pieSolidColors = ['#6259ca', '#00cccc', '#ff9b21', '#fd6074', '#49b6f5', '#01b8ff', '#35b159'];

  constructor(
    private chartService: ChartService,
    private employeeService: EmployeeService,
    private authService: AuthService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    this.fromDate = this.toIsoDate(new Date());

    const roleLevel = this.authService.getRoleLevel();
    this.isAdmin = roleLevel >= 50;

    if (this.isAdmin) {
      this.loadEmployees();
      this.selectedEmployeeId = undefined;
    } else {
      const user = this.authService.getCurrentUser();
      this.selectedEmployeeId = user.userId;
      this.loadCharts();
    }
  }

  onFilterApply(): void {
    if (this.isAdmin && !this.selectedEmployeeId) {
      this.toastr.warning(this.translate.instant('FORM.SELECT'));
      return;
    }
    if (!this.fromDate) {
      this.toastr.warning(this.translate.instant('COMMON.SELECT_FROM_DATE'));
      return;
    }
    this.loadCharts();
  }

  loadCharts(): void {
    if (!this.selectedEmployeeId) return;

    this.isLoading = true;

    this.chartService.getEmployeeTasksChart(this.selectedEmployeeId, this.fromDate, this.toDate)
      .subscribe({
        next: (res) => {
          const tasks: EmpTaskChartItem[] = res?.data?.tasks ?? [];
          const statusCounts: TaskStatusCountDto[] = res?.data?.statusCounts ?? [];

          const barLabels = tasks.map(t => t.taskName);
          const barValues = tasks.map(t => this.parsePercent(t.percent));

          const bg = barValues.map((_, i) => this.colors[i % this.colors.length]);
          const br = barValues.map((_, i) => this.borderColors[i % this.borderColors.length]);

          this.barChartData = {
            labels: barLabels,
            datasets: [{
              label: 'Task %',
              data: barValues,
              backgroundColor: bg,
              borderColor: br,
              borderWidth: 1
            }]
          };

          const pieLabels = statusCounts.map(s =>
            this.translate.instant(this.mapStatus(s.status)));
          const pieData = statusCounts.map(s => s.count);

          this.pieOptions = this.buildPieOptions(pieLabels, pieData);

          this.isLoading = false;
          this.cdr.detectChanges();
        },
        error: () => {
          this.isLoading = false;
          this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
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
        this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
      }
    });
  }


  private buildPieOptions(labels: string[], values: number[]): EChartsOption {
    return {
      tooltip: { trigger: 'item' },
      legend: {
        top: '0%',
        left: 'center',
        textStyle: { color: '#777' }
      },
      series: [
        {
          name: '',
          type: 'pie',
          radius: ['40%', '70%'],
          avoidLabelOverlap: false,
          label: { show: false, position: 'center' },
          emphasis: {
            label: { show: true, fontSize: 17, fontWeight: 'bold' }
          },
          labelLine: { show: false },
          data: labels.map((name, i) => ({
            value: values[i] ?? 0,
            name
          }))
        }
      ],
      color: this.pieSolidColors
    };
  }

  private mapStatus(status: number): string {
  switch (status) {
    case 1: return 'TASK.STATUS_NEW';
    case 2: return 'TASK.STATUS_IN_PROGRESS';
    case 3: return 'TASK.STATUS_CLOSED';
    case 4: return 'TASK.STATUS_ARCHIVED';
    case 5: return 'TASK.STATUS_AUTOCLOSE';
    default: return 'TASK.STATUS_UNKNOWN';
  }
}


  private parsePercent(value: string | null | undefined): number {
    if (!value) return 0;
    const cleaned = value.replace('%', '').trim();
    const n = Number(cleaned);
    return Number.isFinite(n) ? n : 0;
  }

  private toIsoDate(d: Date): string {
    return d.toISOString().slice(0, 10);
  }
}
