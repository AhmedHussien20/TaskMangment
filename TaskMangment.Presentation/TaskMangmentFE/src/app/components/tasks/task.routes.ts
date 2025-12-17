import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { TaskDetailsShellComponent } from './task-details/task-details-shell/task-details-shell.component';

export const TaskRoutes: Routes = [
  {
    path: 'task-list',
    loadComponent: () =>
      import('./task-list/task-list.component').then(m => m.TaskListComponent)
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./task-create-update/task-create-update.component')
        .then(m => m.TaskCreateUpdateComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./task-create-update/task-create-update.component')
        .then(m => m.TaskCreateUpdateComponent)
  },
  {
    path: 'details/:id',
    component: TaskDetailsShellComponent
  }
];
@NgModule({
  imports: [RouterModule.forChild(TaskRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class TaskRoutingModule {
  static routes = TaskRoutes;
}