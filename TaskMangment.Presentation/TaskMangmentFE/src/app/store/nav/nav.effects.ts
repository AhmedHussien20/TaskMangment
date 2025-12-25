import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Store } from '@ngrx/store';
import { TranslateService } from '@ngx-translate/core';
import { map, switchMap, tap } from 'rxjs/operators';
import * as NavActions from './nav.actions';
import { MenuItem } from '../../shared/models/menu-item.model';
import { forkJoin, Observable, of } from 'rxjs';

@Injectable()
export class NavEffects {
  initializeMenu$: any;
  updateTranslations$: any;

private MENUITEMS: MenuItem[] = [

  // ================= Dashboard =================
  { headTitle: 'nav.dashboard.header' },
  {
    title: 'nav.dashboard.title',
    path: '/dashboard',
    type: 'link',
    icon: 'ti-home',
  },

  // ================= System Management =================
  { headTitle: 'nav.applications.header' },
  {
    title: 'nav.apps.title',
    icon: 'ti-layout',
    type: 'sub',
    children: [

      // ---- Organization ----
      {
        title: 'nav.apps.organization.title',
        icon: 'ti-map',
        type: 'sub',
        children: [
          { title: 'nav.apps.area.list', type: 'link', path: '/area/area-list' },
          { title: 'nav.apps.branch.list', type: 'link', path: '/branch/branch-list' },
          { title: 'nav.apps.department.list', type: 'link', path: '/department/department-list' },
        ],
      },

      // ---- Users & Permissions ----
      {
        title: 'nav.apps.employee.title',
        icon: 'ti-user',
        type: 'sub',
        children: [
          { title: 'nav.apps.employee.list', type: 'link', path: '/employee/employee-list' },
          { title: 'nav.apps.role.list', type: 'link', path: '/role/role-list' },
          { title: 'nav.apps.permission.list', type: 'link', path: '/role/permission-list' },
        ],
      },

      // ---- Operations ----
      {
        title: 'nav.apps.operations.title',
        icon: 'ti-clipboard',
        type: 'sub',
        children: [
          { title: 'nav.apps.task.list', type: 'link', path: '/task/task-list' },
          { title: 'nav.apps.calender.title', type: 'link', path: '/utilities/event-calender' },
        ],
      },

      // ---- Education ----
      {
        title: 'nav.apps.education.title',
        icon: 'ti-book',
        type: 'sub',
        children: [
          { title: 'nav.apps.student.list', type: 'link', path: '/student/student-list' },
          { title: 'nav.apps.course.list', type: 'link', path: '/course/course-list' },
        ],
      },

      // ---- Configurations ----
      {
        title: 'nav.apps.configurations.title',
        icon: 'ti-settings',
        type: 'sub',
        children: [
          { title: 'nav.apps.configurations.moduleAvailability', type: 'link', path: '/configuration/modulesAvailabilityList' },
          { title: 'nav.apps.configurations.modules', type: 'link', path: '/configuration/modules' },
          { title: 'nav.apps.configurations.branches', type: 'link', path: '/configuration/branches' },
          { title: 'nav.apps.configurations.onlineOrders', type: 'link' },
          { title: 'nav.apps.configurations.externalIntegration', type: 'link' },
          { title: 'nav.apps.configurations.notifications', type: 'link' },
          { title: 'nav.apps.configurations.campaigns', type: 'link' },
          { title: 'nav.apps.configurations.ads', type: 'link' },
        ],
      },
    ],
  },

];


  constructor(private actions$: Actions, private translate: TranslateService) {
    console.log('NavEffects initialized:', this.actions$);

    this.initializeEffects();
  }

  private initializeEffects() {
    this.initializeMenu$ = createEffect(() =>
      this.actions$.pipe(
        ofType(NavActions.initializeMenu),
        switchMap(() =>
          forkJoin(this.MENUITEMS.map((item) => this.translateMenuItem(item)))
        ),
        map((translatedMenu) =>
          NavActions.updateMenuItems({ items: translatedMenu })
        )
      )
    );
    this.updateTranslations$ = createEffect(() =>
      this.actions$.pipe(
        ofType(NavActions.updateMenuTranslations),
        switchMap(() =>
          forkJoin(this.MENUITEMS.map((item) => this.translateMenuItem(item)))
        ),
        map((translatedMenu) =>
          NavActions.updateMenuItems({ items: translatedMenu })
        )
      )
    );
  }

  private translateMenuItem(item: MenuItem): Observable<MenuItem> {
    if (!item) {
      return of({ headTitle: '', title: '', children: [] }); // Ensure we always return a valid MenuItem
    }

    return this.translate.get([item.title || '', item.headTitle || '']).pipe(
      switchMap((translations) => {
        const newItem: MenuItem = { ...item };

        newItem.title = translations[item.title || ''] || item.title || '';
        newItem.headTitle =
          translations[item.headTitle || ''] || item.headTitle || '';

        if (item.children && item.children.length > 0) {
          return forkJoin(
            item.children.map((child) => this.translateMenuItem(child))
          ).pipe(
            map((translatedChildren) => {
              newItem.children = translatedChildren;
              return newItem;
            })
          );
        }

        return of(newItem);
      })
    );
  }
}
