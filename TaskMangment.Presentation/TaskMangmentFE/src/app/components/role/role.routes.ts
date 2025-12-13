import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core'; 

export const RoleRoutes: Routes = [
  {
    path: 'role-list',
    loadComponent: () =>
      import('./role-list/role-list.component')
        .then(m => m.RoleListComponent)
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./role-create-update/role-create-update.component')
        .then(m => m.RoleCreateUpdateComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./role-create-update/role-create-update.component')
        .then(m => m.RoleCreateUpdateComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(RoleRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class RoleRoutingModule {
  static routes = RoleRoutes;
}
