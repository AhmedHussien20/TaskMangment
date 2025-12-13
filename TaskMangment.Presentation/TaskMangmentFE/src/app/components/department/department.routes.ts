import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

export const DepartmentRoutes: Routes = [
  {
    path: 'department-list',
    loadComponent: () =>
      import('./department-list/department-list.component').then(m => m.DepartmentListComponent)
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./department-create-update/department-create-update.component')
        .then(m => m.DepartmentCreateUpdateComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./department-create-update/department-create-update.component')
        .then(m => m.DepartmentCreateUpdateComponent)
  }
];
@NgModule({
  imports: [RouterModule.forChild(DepartmentRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class DepartmentRoutingModule {
  static routes = DepartmentRoutes;
}