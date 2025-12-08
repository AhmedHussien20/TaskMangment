import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

export const employeeRoutes: Routes = [
  {
    path: '',
    children: [
      {
        path: 'employee-management',
        loadComponent: () =>
          import('./employee-management.component').then((m) => m.EmployeeManagementComponent),
      },
      {
        path: 'employee-form',
        loadComponent: () =>
          import('./employee-form.component').then((m) => m.EmployeeFormComponent),
      },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forChild(employeeRoutes), TranslateModule.forChild()],
  exports: [RouterModule],
})
export class EmployeeRoutingModule {
  static routes = employeeRoutes;
}