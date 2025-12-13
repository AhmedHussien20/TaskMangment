import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core'; 

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
  imports: [RouterModule.forChild(EmployeeRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class EmployeeRoutingModule {
  static routes = EmployeeRoutes;
}
