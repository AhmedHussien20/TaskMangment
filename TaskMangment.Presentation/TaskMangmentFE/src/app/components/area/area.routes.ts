import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

export const AreaRoutes: Routes = [
  {
    path: '',
    children: [
        {
        path: 'area-list',
        loadComponent: () =>
            import('./area-list/area-list.component').then(m => m.AreaListComponent)
        },
    ],
    
  }
  //,
//   {
//     path: 'add',
//     loadComponent: () =>
//       import('./area-add/area-add.component').then(m => m.AreaAddComponent)
//   },
//   {
//     path: 'edit/:id',
//     loadComponent: () =>
//       import('./area-edit/area-edit.component').then(m => m.AreaEditComponent)
//   }
];

@NgModule({
  imports: [RouterModule.forChild(AreaRoutes), TranslateModule.forChild()],
  exports: [RouterModule],
})

export class AreaRoutingModule {
  static routes = AreaRoutes;
}
