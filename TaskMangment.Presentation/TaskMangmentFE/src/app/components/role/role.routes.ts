import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { EmployeeRoleListComponent } from './employee-role-list/employee-role-list.component';

export const RoleRoutes: Routes = [
  {
    path: 'role-list',
    loadComponent: () =>
      import('./role-list/role-list.component')
        .then(m => m.RoleListComponent)
  },
  {
    path: 'create-role',
    loadComponent: () =>
      import('./role-create-update/role-create-update.component')
        .then(m => m.RoleCreateUpdateComponent)
  },
  {
    path: 'permission-list',
    loadComponent: () =>
      import('./permission-list/permission-list.component')
        .then(m => m.PermissionListComponent)
  },
  {
    path: 'create-permission',
    loadComponent: () =>
      import('./permission-create-update/permission-create-update.component')
        .then(m => m.PermissionCreateUpdateComponent)
  },
  {
    path: 'edit-permission/:id',
    loadComponent: () =>
      import('./permission-create-update/permission-create-update.component')
        .then(m => m.PermissionCreateUpdateComponent)
  },
  {
    path: ':roleId/employees',
    component: EmployeeRoleListComponent
  }

];

@NgModule({
  imports: [RouterModule.forChild(RoleRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class RoleRoutingModule {
  static routes = RoleRoutes;
}
