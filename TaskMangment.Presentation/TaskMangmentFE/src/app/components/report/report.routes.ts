import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthGuard } from 'app/core/auth/auth.guard';

/** Any authenticated user can open reports; API scopes the data. */
const authOnly = { canActivate: [AuthGuard] };

const companyReportPermissions = {
  canActivate: [AuthGuard],
  data: { permissions: ['VIEW_COMPANY_REPORTS'] }
};

export const ReportRoutes: Routes = [

  {
    path: 'reports-dashboard',
    ...authOnly,
    loadComponent: () =>
      import('./reports-dashboard/reports-dashboard.component')
        .then(m => m.ReportsDashboardComponent)
  },

  {
    path: 'top-commenter-list',
    ...authOnly,
    loadComponent: () =>
      import('./top-commenter-list/top-commenter-list.component')
        .then(m => m.TopCommenterListComponent)
  },

  {
    path: 'most-assigned-list',
    ...authOnly,
    loadComponent: () =>
      import('./most-assigned-list/most-assigned-list.component')
        .then(m => m.MostAssignedListComponent)
  },

  {
    path: 'on-time-completion-list',
    ...authOnly,
    loadComponent: () =>
      import('./on-time-completion-list/on-time-completion-list.component')
        .then(m => m.OnTimeCompletionListComponent)
  },

  {
    path: 'archived-tasks-list',
    ...authOnly,
    loadComponent: () =>
      import('./archived-tasks-list/archived-tasks-list.component')
        .then(m => m.ArchivedTasksListComponent)
  },

  {
    path: 'tasks-discount-list',
    ...authOnly,
    loadComponent: () =>
      import('./tasks-discoun-report/tasks-discoun-report.component')
        .then(m => m.TasksDiscountReportComponent)
  },
   {
    path: 'tasks-comment-activity-list',
    ...authOnly,
    loadComponent: () =>
      import('./task-comments-activity-report/task-comments-activity-report.component')
        .then(m => m.TaskCommentsActivityReportComponent)
  },
   {
    path: 'tasks-today-activity-list',
    ...authOnly,
    loadComponent: () =>
      import('./task-today-activity-report/task-today-activity-report.component')
        .then(m => m.TaskTodayActivityReportComponent)
  },
  {
    path: 'tasks-closed-soon-list',
    ...authOnly,
    loadComponent: () =>
      import('./task-closed-soon-report/task-closed-soon-report.component')
        .then(m => m.TaskClosedSoonReportComponent)
  },
  {
    path: 'employee-task-tracking',
    ...authOnly,
    loadComponent: () =>
      import('./employee-task-tracking/employee-task-tracking.component')
        .then(m => m.EmployeeTaskTrackingComponent)  },

  {
    path: 'branch-task-tracking',
    ...authOnly,
    loadComponent: () =>
      import('./branch-task-tracking/branch-task-tracking.component')
        .then(m => m.BranchTaskTrackingComponent)  }
  ,
  {
    path: 'employee-task-comments',
    ...authOnly,
    loadComponent: () =>
      import('./employee-task-comments-report/employee-task-comments-report.component')
        .then(m => m.EmployeeTaskCommentsReportComponent)
  },
  {
    path: 'employee-total-discounts',
    ...companyReportPermissions,
    loadComponent: () =>
      import('./employee-total-discounts/employee-total-discounts.component')
        .then(m => m.EmployeeTotalDiscountsComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(ReportRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class ReportRoutingModule {
  static routes = ReportRoutes;
}
