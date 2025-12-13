import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

export const BranchRoutes: Routes = [
  {
    path: 'branch-list',
    loadComponent: () =>
      import('./branch-list/branch-list.component')
        .then(m => m.BranchListComponent)
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./branch-create-update.component/branch-create-update.component')
        .then(m => m.BranchCreateUpdateComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./branch-create-update.component/branch-create-update.component')
        .then(m => m.BranchCreateUpdateComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(BranchRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class BranchRoutingModule {
  static routes = BranchRoutes;
}
