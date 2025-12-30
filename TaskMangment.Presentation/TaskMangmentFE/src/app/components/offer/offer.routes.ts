import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

export const OfferRoutes: Routes = [
  {
    path: 'offer-list',
    loadComponent: () =>
      import('./offer-list/offer-list.component').then(m => m.OfferListComponent)
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./offer-create-update/offer-create-update.component')
        .then(m => m.OfferCreateUpdateComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./offer-create-update/offer-create-update.component')
        .then(m => m.OfferCreateUpdateComponent)
  }
];
@NgModule({
  imports: [RouterModule.forChild(OfferRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class OfferRoutingModule {
  static routes = OfferRoutes;
}