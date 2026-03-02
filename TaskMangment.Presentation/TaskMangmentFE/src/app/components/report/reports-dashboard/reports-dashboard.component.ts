import { Component, OnInit } from '@angular/core'; // تغيير من Component فقط إلى Component, OnInit
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { SpkDashboardComponent } from 'app/@spk/reusable-dashboard/spk-dashboard/spk-dashboard.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { AuthService } from 'app/core/services/auth.service'; // إضافة هذا السطر

interface ReportTile {
  title: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-reports-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, SpkDashboardComponent, PageHeaderComponent],
  templateUrl: './reports-dashboard.component.html',
  styleUrls: ['./reports-dashboard.component.css']
})
export class ReportsDashboardComponent implements OnInit { // إضافة implements OnInit

  title = 'MENU.REPORTS';
  activeitem = 'MENU.REPORTS';
  breadcrumbs = ['MENU.HOME', 'MENU.REPORTS'];
  isLoading = false;
  
  // تعريف متغير للكروت المصفاة
  filteredCards: any[] = [];
  
  REPORT_ICON_SVG = `
<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
     viewBox="0 0 24 24" fill="none" stroke="currentColor"
     stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
     class="feather feather-file-text text-primary">
  <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
  <polyline points="14 2 14 8 20 8"></polyline>
  <line x1="16" y1="13" x2="8" y2="13"></line>
  <line x1="16" y1="17" x2="8" y2="17"></line>
  <polyline points="10 9 9 9 8 9"></polyline>
</svg>
`;

  allCards = [
    {
      title: 'REPORTS.TOP_COMMENTER',
      value: '',
      svg: this.REPORT_ICON_SVG,
      clickable: true,
      url: '/report/top-commenter-list'
    },
    {
      title: 'REPORTS.MOST_ASSIGNED',
      value: '',
      svg: this.REPORT_ICON_SVG,
      clickable: true,
      url: '/report/most-assigned-list'
    },
    {
      title: 'REPORTS.ON_TIME',
      value: '',
      svg: this.REPORT_ICON_SVG,
      clickable: true,
      url: '/report/on-time-completion-list'
    },
    {
      title: 'REPORTS.ARCHIVED',
      value: '',
      svg: this.REPORT_ICON_SVG,
      clickable: true,
      url: '/report/archived-tasks-list'
    },
    {
      title: 'REPORTS.TASKS_DISCOUNT',
      value: '',
      svg: this.REPORT_ICON_SVG,
      clickable: true,
      url: '/report/tasks-discount-list'
    },
    {
      title: 'REPORTS.TASK_ACTIVITY_REPORT',
      value: '',
      svg: this.REPORT_ICON_SVG,
      clickable: true,
      url: '/report/tasks-comment-activity-list'
    },
    {
      title: 'REPORTS.TASK_MOVEMENT_TITLE',
      value: '',
      svg: this.REPORT_ICON_SVG,
      clickable: true,
      url: '/report/tasks-today-activity-list'
    },
    {
      title: 'REPORTS.TASKS_CLOSING_SOON',
      value: '',
      svg: this.REPORT_ICON_SVG,
      clickable: true,
      url: '/report/tasks-closed-soon-list'
    },
    {
      title: 'REPORTS.EMPLOYEE_TASK_TRACKING',
      value: '',
      svg: this.REPORT_ICON_SVG,
      clickable: true,
      url: '/report/employee-task-tracking'
    },
    {
      title: 'REPORTS.BRANCH_TASK_TRACKING',
      value: '',
      svg: this.REPORT_ICON_SVG,
      clickable: true,
      url: '/report/branch-task-tracking'
    }
  ];

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.filterCardsByRole();
  }

  private filterCardsByRole(): void {
  const roleLevel = this.authService.getRoleLevel() ?? 0;
  
  if (roleLevel >= 70) {
    this.filteredCards = [...this.allCards];
  } else if (this.authService.hasPermission('CREATE_TASK')) {
    this.filteredCards = this.allCards.filter(card => card.title !== 'REPORTS.BRANCH_TASK_TRACKING');
  } else {
    const restrictedCards = [
      'REPORTS.TOP_COMMENTER',
      'REPORTS.MOST_ASSIGNED',
      'REPORTS.ARCHIVED',
      'REPORTS.BRANCH_TASK_TRACKING'
    ];

    this.filteredCards = this.allCards.filter(card => !restrictedCards.includes(card.title));
  }
}
}