import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core'; 

export const LeaveRoutes: Routes = [
  {
    path: 'leave-list',
    loadComponent: () =>
      import('./leave-management/leave-list/leave-list.component')
        .then(m => m.LeaveListComponent)
  },
  {
    path: 'leave-create',
    loadComponent: () =>
      import('./leave-management/leave-create-update/leave-create-update.component')
        .then(m => m.LeaveCreateUpdateComponent)
  },
  {
    path: 'leave-type-list',
    loadComponent: () =>
      import('./leave-type/leave-type/leave-type.component')
        .then(m => m.LeaveTypeComponent)
  },
];

@NgModule({
  imports: [RouterModule.forChild(LeaveRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class LeaveRoutingModule {
  static routes = LeaveRoutes;
}
