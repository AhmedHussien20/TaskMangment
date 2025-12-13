import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core'; 

export const StudentRoutes: Routes = [
  {
    path: 'student-list',
    loadComponent: () =>
      import('./student-list/student-list.component')
        .then(m => m.StudentListComponent)
  },
  {
    path: 'add',
    loadComponent: () =>
      import('./student-create-update/student-create-update.component')
        .then(m => m.StudentCreateUpdateComponent)
  },
  {
    path: 'edit/:id',
    loadComponent: () =>
      import('./student-create-update/student-create-update.component')
        .then(m => m.StudentCreateUpdateComponent)
  }
];

@NgModule({
  imports: [RouterModule.forChild(StudentRoutes), TranslateModule.forChild()],
  exports: [RouterModule]
})
export class StudentRoutingModule {
  static routes = StudentRoutes;
}
