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
    { headTitle: 'nav.dashboard.header' },
    {
      title: 'nav.dashboard.title',
      path: '/dashboard',
      type: 'link',
      icon: 'ti-home',
      active: false,
      selected: false,
      dirchange: false,
    },

    { headTitle: 'nav.applications.header' },
    {
      title: 'nav.apps.title',
      icon: 'ti-write',
      type: 'sub',
      active: false,
      selected: false,
      dirchange: false,
      children: [
        {
          title: 'nav.apps.area.title',
          icon: 'ti-write',
          type: 'sub',
          path: '/area',
          active: false,
          selected: false,
          dirchange: false,
          children: [
            {
              title: 'nav.apps.area.list',
              type: 'link',
              path: '/area/area-list',
              dirchange: false,
            }
             
          ],
        },
        {
          title: 'nav.apps.branch.title',
          icon: 'ti-write',
          type: 'sub',
          path: '/branch',
          active: false,
          selected: false,
          dirchange: false,
          children: [
            {
              title: 'nav.apps.branch.list',
              type: 'link',
              path: '/branch/branch-list',
              dirchange: false,
            } 
          ],
        },
        {
          title: 'nav.apps.department.title',
          icon: 'ti-write',
          type: 'sub',
          active: false,
          selected: false,
          dirchange: false,
          children: [
            {
              title: 'nav.apps.department.list',
              type: 'link',
               path: '/department/department-list',
              dirchange: false,
            } 
          ],
        },
        {
          title: 'nav.apps.task.title',
          icon: 'ti-write',
          type: 'sub',
          active: false,
          selected: false,
          dirchange: false,
          children: [
            {
              title: 'nav.apps.task.list',
              type: 'link',
              path: '/task/task-list',
              dirchange: false,
            },
          ],
        },
        {
          title: 'nav.apps.group.title',
          icon: 'ti-write',
          type: 'sub',
          path: '/group',
          active: false,
          selected: false,
          dirchange: false,
          children: [
            { title: 'nav.apps.group.list', type: 'link', path: '/group/list', dirchange: false },
          ],
        },
        {
          title: 'nav.apps.configurations.title',
          icon: 'ti-write',
          type: 'sub',
          path: '/configuration',
          active: false,
          selected: false,
          dirchange: false,
          children: [
            {
              title: 'nav.apps.configurations.onlineOrders',
              type: 'link',
              dirchange: false,
            },
            {
              title: 'nav.apps.configurations.expiry',
              type: 'link',
              dirchange: false,
            },
            {
              title: 'nav.apps.configurations.externalIntegration',
              type: 'link',
              dirchange: false,
            },
            {
              title: 'nav.apps.configurations.moduleAvailability',
              type: 'link',
              path: '/configuration/modulesAvailabilityList',
              dirchange: false,
            },
            {
              title: 'nav.apps.configurations.modules',
              type: 'link',
              path: '/configuration/modules',
              dirchange: false,
            },
            {
              title: 'nav.apps.configurations.branches',
              type: 'link',
              path: '/configuration/branches',
              dirchange: false,
            },
            {
              title: 'nav.apps.configurations.campaigns',
              type: 'link',
              dirchange: false,
            },
            {
              title: 'nav.apps.configurations.ads',
              type: 'link',
              dirchange: false,
            },
            {
              title: 'nav.apps.configurations.notifications',
              type: 'link',
              dirchange: false,
            },
          ],
        },
      ],
    },

    { headTitle: 'nav.components.header' },
    {
      title: 'nav.components.submenus.title',
      icon: 'ti-panel',
      type: 'sub',
      active: false,
      dirchange: false,
      children: [
        {
          title: 'nav.components.submenus.level1',
          type: 'empty',
          dirchange: false,
        },
        {
          title: 'nav.components.submenus.level2.title',
          type: 'sub',
          dirchange: false,
          children: [
            {
              title: 'nav.components.submenus.level2.level2_0',
              type: 'empty',
              dirchange: false,
            },
            {
              title: 'nav.components.submenus.level2.level2_1',
              type: 'empty',
              dirchange: false,
            },
            {
              title: 'nav.components.submenus.level2.level2_2.title',
              type: 'sub',
              active: false,
              dirchange: false,
              children: [
                {
                  title: 'nav.components.submenus.level2.level2_2.level2_2_1',
                  type: 'empty',
                  dirchange: false,
                },
                {
                  title: 'nav.components.submenus.level2.level2_2.level2_2_2',
                  type: 'empty',
                  dirchange: false,
                },
              ],
            },
          ],
        },
        {
          title: 'nav.components.submenus.level3',
          type: 'empty',
          dirchange: false,
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
