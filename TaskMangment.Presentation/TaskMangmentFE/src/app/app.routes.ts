import { Route } from '@angular/router';

import { FullLayoutComponent } from './shared/layouts/full-layout/full-layout.component';
import { content } from './shared/routes/full-content.routes';
import { MessageLayoutComponent } from './shared/layouts/message-layout/message-layout.component';
import { Authen_Routes, Message_Routes } from './shared/routes/content.routes';
import { ContentLayoutComponent } from './shared/layouts/content-layout/content-layout.component';
import { LandingpageLayoutComponent } from './shared/layouts/landingpage-layout/landingpage-layout.component';
import { landing } from './shared/routes/landingpage';
import { AuthGuard } from './core/auth/auth.guard';
import { platformAuthGuard, platformGuestGuard, platformRoleGuard } from './platform/platform.guard';

export const App_Route: Route[] = [
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },
  {
    path: 'auth',
    loadChildren: () => import('./authentication/authentications.routes').then(m => m.authenticationsRoutingModule)
  } ,
  {
    path: 'platform',
    children: [
      {
        path: 'login',
        canActivate: [platformGuestGuard],
        loadComponent: () => import('./platform/login/platform-login.component').then(m => m.PlatformLoginComponent)
      },
      {
        path: '',
        canActivate: [platformAuthGuard],
        loadComponent: () => import('./platform/layout/platform-layout.component').then(m => m.PlatformLayoutComponent),
        children: [
          {
            path: 'admins',
            canActivate: [platformRoleGuard],
            data: { roles: ['SuperAdmin'] },
            loadComponent: () => import('./platform/admins/platform-admins.component').then(m => m.PlatformAdminsComponent)
          },
          {
            path: 'institutes',
            canActivate: [platformRoleGuard],
            data: { roles: ['SuperAdmin', 'PlatformAdmin'] },
            loadComponent: () => import('./platform/institutes/platform-institutes.component').then(m => m.PlatformInstitutesComponent)
          },
          {
            path: 'trainees',
            canActivate: [platformRoleGuard],
            data: { roles: ['SuperAdmin', 'PlatformAdmin', 'OrganizationAdmin'] },
            loadComponent: () => import('./platform/trainees/platform-trainees.component').then(m => m.PlatformTraineesComponent)
          }
        ]
      }
    ]
  },
  {
    path: '',
    component: FullLayoutComponent,
    children: content,
    // canActivate: [AuthGuard]
  },
  { path: '', component: MessageLayoutComponent, children: Message_Routes },

  { path: '', component: ContentLayoutComponent, children: Authen_Routes },
  { path: '', component: LandingpageLayoutComponent, children: landing }

];