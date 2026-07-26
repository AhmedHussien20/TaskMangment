import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core'; 
import { AuthGuard } from 'app/core/auth/auth.guard';

export const EmployeeRoutes: Routes = [
  {
    path: 'employee-list',
    loadComponent: () =>
      import('./employee-list/employee-list.component')
        .then(m => m.EmployeeListComponent)
  },
  {
    path: '360/:id',
    loadComponent: () =>
      import('./employee-360/employee-360.component')
        .then(m => m.Employee360Component)
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./employee-create-update/employee-create-update.component')
        .then(m => m.EmployeeCreateUpdateComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./employee-create-update/employee-create-update.component')
        .then(m => m.EmployeeCreateUpdateComponent),
  }
];

@NgModule({
  imports: [RouterModule.forChild(EmployeeRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class EmployeeRoutingModule {
  static routes = EmployeeRoutes;
}
