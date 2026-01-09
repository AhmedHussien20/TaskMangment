import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { SpkDashboardComponent } from 'app/@spk/reusable-dashboard/spk-dashboard/spk-dashboard.component';

interface ReportTile {
  title: string;
  icon: string;
  route: string;
}

@Component({
  selector: 'app-reports-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule, TranslateModule, SpkDashboardComponent],
  templateUrl: './reports-dashboard.component.html',
  styleUrls: ['./reports-dashboard.component.css']
})
export class ReportsDashboardComponent {

  isLoading = false;
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
 cards = [
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
    title: 'REPORTS.DISCOUNT',
    value: '',
     svg: this.REPORT_ICON_SVG,
    clickable: true,
    url: '/report/tasks-discount-list'
  }
];


}

