import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { employeeRoutes } from '../employee-management/employee.routes';

export const EmployeeRoutes: Routes = [
  {
    path: 'employee-list',
    loadComponent: () =>
      import('./employee-list/employee-list.component')
        .then(m => m.EmployeeListComponent)
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
        .then(m => m.EmployeeCreateUpdateComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(employeeRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class EmployeeRoutingModule {
  static routes = employeeRoutes;
}
