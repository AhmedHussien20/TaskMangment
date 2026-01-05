import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core'; 

export const ReportRoutes: Routes = [
  {
    path: 'Commenter-list',
    loadComponent: () =>
      import('./top-commenter-list/top-commenter-list.component')
        .then(m => m.TopCommenterListComponent)
  },
  {
    path: 'Assigned-employee-list',
    loadComponent: () =>
      import('./most-assigned-list/most-assigned-list.component')
        .then(m => m.MostAssignedListComponent)
  },
  {
    path: 'Ontime-completion-list',
    loadComponent: () =>
      import('./on-time-completion-list/on-time-completion-list.component')
        .then(m => m.OnTimeCompletionListComponent)
  },
  {
    path: 'Archived-tasks-list',
    loadComponent: () =>
      import('./archived-tasks-list/archived-tasks-list.component')
        .then(m => m.ArchivedTasksListComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(ReportRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class TopCommenterRoutingModule {
  static routes = ReportRoutes;
}
