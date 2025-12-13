import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

export const admin: Routes = [
  {
    path: '', children: [{
      path: 'dashboard',
      loadComponent: () =>
        import('./dashboard/dashboard.component').then((m) => m.DashboardComponent),
    },
    ]
  }
];
@NgModule({
  imports: [RouterModule.forChild(admin), TranslateModule.forChild()],
  exports: [RouterModule],
})
export class dashboardRoutingModule {
  static routes = admin;
}