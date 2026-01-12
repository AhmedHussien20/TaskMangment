import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core'; 

export const ReportRoutes: Routes = [

  {
    path: 'reports-dashboard',
    loadComponent: () =>
      import('./reports-dashboard/reports-dashboard.component')
        .then(m => m.ReportsDashboardComponent)
  },

  {
    path: 'top-commenter-list',
    loadComponent: () =>
      import('./top-commenter-list/top-commenter-list.component')
        .then(m => m.TopCommenterListComponent)
  },

  {
    path: 'most-assigned-list',
    loadComponent: () =>
      import('./most-assigned-list/most-assigned-list.component')
        .then(m => m.MostAssignedListComponent)
  },

  {
    path: 'on-time-completion-list',
    loadComponent: () =>
      import('./on-time-completion-list/on-time-completion-list.component')
        .then(m => m.OnTimeCompletionListComponent)
  },

  {
    path: 'archived-tasks-list',
    loadComponent: () =>
      import('./archived-tasks-list/archived-tasks-list.component')
        .then(m => m.ArchivedTasksListComponent)
  },

  {
    path: 'tasks-discount-list',
    loadComponent: () =>
      import('./tasks-discoun-report/tasks-discoun-report.component')
        .then(m => m.TasksDiscountReportComponent)
  },
   {
    path: 'tasks-comment-activity-list',
    loadComponent: () =>
      import('./task-comments-activity-report/task-comments-activity-report.component')
        .then(m => m.TaskCommentsActivityReportComponent)
  },
   {
    path: 'tasks-today-activity-list',
    loadComponent: () =>
      import('./task-today-activity-report/task-today-activity-report.component')
        .then(m => m.TaskTodayActivityReportComponent)
  },
  {
    path: 'tasks-closed-soon-list',
    loadComponent: () =>
      import('./task-closed-soon-report/task-closed-soon-report.component')
        .then(m => m.TaskClosedSoonReportComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(ReportRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class ReportRoutingModule {
  static routes = ReportRoutes;
}
