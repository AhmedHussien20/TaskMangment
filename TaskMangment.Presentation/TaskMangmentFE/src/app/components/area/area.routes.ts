import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
 

export const AreaRoutes: Routes = [
  {
    path: 'area-list',
    loadComponent: () =>
      import('./area-list/area-list.component').then(m => m.AreaListComponent)
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./area-create-update.component/area-create-update.component.component')
        .then(m => m.AreaCreateUpdateComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./area-create-update.component/area-create-update.component.component')
        .then(m => m.AreaCreateUpdateComponent)
  }
];


@NgModule({
  imports: [RouterModule.forChild(AreaRoutes), TranslateModule.forChild()],
  exports: [RouterModule],
})
export class AreaRoutingModule {
  static routes = AreaRoutes;
}
