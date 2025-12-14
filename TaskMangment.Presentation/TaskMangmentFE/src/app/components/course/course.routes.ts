import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
 

export const CourseRoutes: Routes = [
  {
    path: 'course-list',
    loadComponent: () =>
      import('./course-list/course-list.component').then(m => m.CourseListComponent)
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./course-create-update/course-create-update.component').then(m => m.CourseCreateUpdateComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./course-create-update/course-create-update.component').then(m => m.CourseCreateUpdateComponent)
  }
];


@NgModule({
  imports: [RouterModule.forChild(CourseRoutes), TranslateModule.forChild()],
  exports: [RouterModule],
})
export class CourseRoutingModule {
  static routes = CourseRoutes;
}
