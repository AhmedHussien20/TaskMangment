import { ChangeDetectorRef, Component, ElementRef, HostListener, OnInit, Renderer2, inject, OnDestroy } from '@angular/core';
import { filter, Subscription } from 'rxjs';
import { LayoutService } from '../../services/layout.service';
import { NavService } from '../../services/nav.service';
import { SwitcherService } from '../../services/switcher.service';
import { NgbModal, NgbOffcanvas } from '@ng-bootstrap/ng-bootstrap';
import { SwitcherComponent } from '../switcher/switcher.component';
import { AppStateService } from '../../services/app-state.service';
import { MenuItem } from '../../models/menu-item.model';
import { AuthService } from 'app/core/services/auth.service';
import { TranslationService } from 'app/shared/services/translation.service';
import { SignalRService } from 'app/core/services/signalr.service';
import { NotificationApiService } from 'app/core/services/notification.service';
import { Router } from '@angular/router';
import { TaskDetailsShellComponent } from 'app/components/tasks/task-details/task-details-shell/task-details-shell.component';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';

interface Item {
  user: any;

  id: number;
  name: string;
  type: string;
  title: string;
  // Add other properties as needed
}
export interface HeaderShortcut {
  title: string;        // translation key
  icon: string;         // icon class
  path: string;
  alwaysEnabled?: boolean;

}
export interface HeaderNotification {
  id: number;
  message: string;
  createdAt: Date;
  isRead: boolean;
  link: string;
  type?: 'task' | 'message' | 'other';
  taskId?: number;
}


@Component({
  selector: 'app-header',
  standalone: false,
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
  
})
export class HeaderComponent implements OnInit, OnDestroy {
  user: any;
  defaultAvatar = 'assets/images/user.png';
  notifications: HeaderNotification[] = [];

  notificationCount = 0;
  private notificationSubscription$?: Subscription;
  private unreadChangedSubscription$?: Subscription;

  get profileImage(): string {
    if (this.user?.profileImage) {
      return this.user.profileImage;
    }

    return this.defaultAvatar;
  }

  roleLevel = 0;

  canAccessShortcut(item: HeaderShortcut): boolean {

    if (this.roleLevel >= 50) return true;

    return item.alwaysEnabled === true;
  }


  headerShortcuts: HeaderShortcut[] = [
    {
      title: 'nav.apps.task.title',
      icon: 'ti-check-box',
      path: '/task/task-list',
      alwaysEnabled: true
    },
    {
      title: 'nav.apps.employee.title',
      icon: 'ti-user',
      path: '/employee/employee-list'
    },
    {
      title: 'nav.apps.branch.title',
      icon: 'ti-map-alt',
      path: '/branch/branch-list'
    },
    {
      title: 'nav.apps.department.title',
      icon: 'ti-layers',
      path: '/department/department-list'
    },
    {
      title: 'nav.apps.calender.title',
      icon: 'ti-calendar',
      path: '/utilities/event-calender',
      alwaysEnabled: true
    },
    {
      title: 'nav.apps.student.title',
      icon: 'ti-id-badge',
      path: '/student/student-list'
    }
    ,
    {
      title: 'nav.apps.reports.title',
      icon: 'ti-id-badge',
      path: '/report/reports-dashboard',
      alwaysEnabled: true
    },
    {
      title: 'nav.apps.leaves.title',
      icon: 'ti-id-badge',
      path: '/leave/leave-list',
      alwaysEnabled: true
    }
  ];
  ;

  Selection = [
    { label: 'Choose one', value: 1 },
    { label: 'T-Projects...', value: 2 },
    { label: 'Microsoft Project', value: 3 },
    { label: 'Risk Management', value: 4 },
    { label: 'Team Building', value: 5 },
  ]
  private offcanvasService = inject(NgbOffcanvas);


  open() {
    this.offcanvasService.open(SwitcherComponent, {
      position: 'end',
      scroll: true,
      panelClass: 'switcher-canvas-width'
    });
  }
  cartItemCount: number = 5;
  public isCollapsed = true;
  collapse: any;
  public isSidebar = false;
  public config: any = {};
  layoutSubscription: Subscription;
  toggleClass = "fe fe-maximize";
  isFullscreen: boolean = false;
  menuItems: MenuItem[] = [];
  menuitemsSubscribe$!: Subscription;
  countryFlag: string = 'gb';

  constructor(
    private cdr: ChangeDetectorRef,
    private layoutService: LayoutService,
    public navServices: NavService,
    public modalService: NgbModal,
    public switcherService: SwitcherService,
    private elementRef: ElementRef,
    public renderer: Renderer2,
    private appStateService: AppStateService,
    private authService: AuthService,
    private translate: TranslationService,
    private signalR: SignalRService,
    private notificationService: NotificationApiService,
    private router: Router,
    private toastr: ToastrService,
    private translationService: TranslateService
  ) {
    this.layoutSubscription = layoutService.changeEmitted.subscribe(
      direction => {
        const dir = direction.direction;
      }
    )

  }



  categories = [
    { id: 1, name: 'IT Projects' },
    { id: 2, name: 'Business Case' },
    { id: 3, name: 'Microsoft Project' },
    { id: 4, name: 'Risk Management' },
    { id: 5, name: 'Team Building' },
  ]

  toggleSidebarNotification() {
    this.layoutService.emitSidebarNotifyChange(true);
  }

  changeLanguage(lang: string) {
    this.translate.setLanguage(lang);
  }

  mapLangToFlag(lang: string): string {
    switch (lang) {
      case 'en':
        return 'gb';
      case 'ar':
        return 'sa';
      case 'es':
      case 'fr':
      case 'de':
      case 'it':
      case 'ru':
        return lang;
      default:
        return 'gb'; // Default to English flag
    }
  }


  toggleSwitcher() {
    let emit: any = false;
    if (emit != !emit) {
      emit = !emit
      this.switcherService.emitSwitcherChange(emit);
    }
  }
  // Theme color Mode

  isCartEmpty: boolean = false;
  isNotifyEmpty: boolean = false;

  removeRow(rowId: string) {
    const rowElement = document.getElementById(rowId);
    if (rowElement) {
      rowElement.remove();


    }
    this.cartItemCount--;
    this.isCartEmpty = this.cartItemCount === 0;

  }
  removeNotify(rowId: string) {
    const rowElement = document.getElementById(rowId);
    if (rowElement) {
      rowElement.remove();


    }
    this.notificationCount--;
    this.isNotifyEmpty = this.notificationCount === 0;
  }
  handleCardClick(event: Event): void {
    // Prevent the click event from propagating to the container
    // event.preventDefault();
    event.stopPropagation();
  }
  themeType = 'dark';
  updateTheme(theme: string) {
    this.appStateService.updateState({ theme, menuColor: theme });
    if (theme == 'light') {
      this.appStateService.updateState({ theme, themeBackground: '', headerColor: 'light', menuColor: 'dark' });
      let html = document.querySelector('html');
      html?.style.removeProperty('--body-bg-rgb');
      html?.style.removeProperty('--body-bg-rgb2');
      html?.style.removeProperty('--light-rgb');
      html?.style.removeProperty('--form-control-bg');
      html?.style.removeProperty('--input-border');
      html?.style.removeProperty('--sidemenu-active-bgcolor');
    }
    if (theme == 'dark') {
      this.appStateService.updateState({ theme, themeBackground: '', headerColor: 'dark', menuColor: 'dark' });
      let html = document.querySelector('html');
      html?.style.removeProperty('--body-bg-rgb');
      html?.style.removeProperty('--body-bg-rgb2');
      html?.style.removeProperty('--light-rgb');
      html?.style.removeProperty('--form-control-bg');
      html?.style.removeProperty('--input-border');
      html?.style.removeProperty('--sidemenu-active-bgcolor');

    }
    if (window.innerWidth <= 992) {
      let html = document.querySelector('html');
      html?.setAttribute('data-toggled', 'close');
    }
  }
  toggleFullscreen() {
    if (this.isFullscreen) {
      this.exitFullscreen();
    } else {
      this.requestFullscreen();
    }
  }

  @HostListener('document:fullscreenchange', ['$event'])
  handleFullscreenChange(event: any) {
    this.isFullscreen = this.isFullScreen();
    this.cdr.detectChanges(); // Manually trigger change detection
  }


  private isFullScreen(): boolean {
    return !!document.fullscreenElement;
  }

  private requestFullscreen() {
    const elem = document.documentElement as any;
    if (elem.requestFullscreen) {
      elem.requestFullscreen();
    }
  }

  private exitFullscreen() {
    if (document.exitFullscreen) {
      document.exitFullscreen();
    }
  }
  SwicherOpen() {
    document.querySelector('.offcanvas-end')?.classList.toggle('show')
    document.querySelector("body")!.classList.add("overflow:hidden");
    document.querySelector("body")!.classList.add("padding-right:4px");
    const Rightside: any = document.querySelector(".offcanvas-end");
    if (document.querySelector(".switcher-backdrop")?.classList.contains('d-none')) {
      document.querySelector(".switcher-backdrop")?.classList.add("d-block");
      document.querySelector(".switcher-backdrop")?.classList.remove("d-none");
    }
  }

  togglesidebar() {
    let html = this.elementRef.nativeElement.ownerDocument.documentElement;
    if (html?.getAttribute('data-toggled') == 'true') {
      document.querySelector('html')?.getAttribute('data-toggled') ==
        'icon-overlay-close';
    }
    else if (html?.getAttribute('data-nav-style') == 'menu-click') {
      html?.setAttribute(
        'data-toggled',
        html?.getAttribute('data-toggled') == 'menu-click-closed'
          ? ''
          : 'menu-click-closed'
      );
    } else if (html?.getAttribute('data-nav-style') == 'menu-hover') {
      html?.setAttribute(
        'data-toggled',
        html?.getAttribute('data-toggled') == 'menu-hover-closed'
          ? ''
          : 'menu-hover-closed'
      );
    } else if (html?.getAttribute('data-nav-style') == 'icon-click') {
      html?.setAttribute(
        'data-toggled',
        html?.getAttribute('data-toggled') == 'icon-click-closed'
          ? ''
          : 'icon-click-closed'
      );
    } else if (html?.getAttribute('data-nav-style') == 'icon-hover') {
      html?.setAttribute(
        'data-toggled',
        html?.getAttribute('data-toggled') == 'icon-hover-closed'
          ? ''
          : 'icon-hover-closed'
      );
    }
    else if (html?.getAttribute('data-vertical-style') == 'overlay') {
      html?.setAttribute(
        'data-vertical-style', 'overlay'
      );
      html?.setAttribute(
        'data-toggled', html?.getAttribute('data-toggled') == 'icon-overlay-close'
        ? ''
        : 'icon-overlay-close'
      );
    } else if (html?.getAttribute('data-vertical-style') == 'overlay') {
      document.querySelector('html')?.getAttribute('data-toggled') != null
        ? document.querySelector('html')?.removeAttribute('data-toggled')
        : document
          .querySelector('html')
          ?.setAttribute('data-toggled', 'icon-overlay-close');
    } else if (html?.getAttribute('data-vertical-style') == 'closed') {
      html?.setAttribute(
        'data-toggled',
        html?.getAttribute('data-toggled') == 'close-menu-close'
          ? ''
          : 'close-menu-close'
      );
    } else if (html?.getAttribute('data-vertical-style') == 'icontext') {
      html?.setAttribute(
        'data-toggled',
        html?.getAttribute('data-toggled') == 'icon-text-close'
          ? ''
          : 'icon-text-close'
      );
    } else if (html?.getAttribute('data-vertical-style') == 'detached') {
      html?.setAttribute(
        'data-toggled',
        html?.getAttribute('data-toggled') == 'detached-close'
          ? ''
          : 'detached-close'
      );
    } else if (html?.getAttribute('data-vertical-style') == 'doublemenu') {
      html?.setAttribute('data-toggled', html?.getAttribute('data-toggled') == 'double-menu-close' && document.querySelector(".slide.open")?.classList.contains("has-sub") && document.querySelector('.double-menu-active') ? 'double-menu-open' : 'double-menu-close');
    }

    if (window.innerWidth <= 992) {
      html?.setAttribute(
        'data-toggled',
        html?.getAttribute('data-toggled') == 'open' ? 'close' : 'open'
      );
    }
  }
  rightsidebar() {
    document.querySelector(".right-sidebar-canvas")?.classList.toggle("show");
    document.querySelector("body")!.classList.add("overflow:hidden");
    document.querySelector("body")!.classList.add("padding-right:4px");
    if (document.querySelector(".right-backdrop")?.classList.contains('d-none')) {
      document.querySelector(".right-backdrop")?.classList.add("d-block");
      document.querySelector(".right-backdrop")?.classList.remove("d-none");
    }
  }

  // Search
  public items!: MenuItem[];
  public text!: string;
  public SearchResultEmpty: boolean = false;


 ngOnInit(): void {

  this.user = this.authService.getCurrentUser();
  this.roleLevel = this.authService.getRoleLevel();

  this.menuitemsSubscribe$ = this.navServices.getMenuItems().subscribe({
    next: (menuItems) => {
      if (menuItems) {
        this.menuItems = menuItems;
      }
    },
    error: (err) => {
      console.error('Error fetching menu items:', err);
    }
  });

  this.translate.getCurrentLang().subscribe(lang => {
    this.countryFlag = this.mapLangToFlag(lang);
  });


  this.loadUnreadNotifications();

  this.unreadChangedSubscription$ = this.notificationService.unreadChanged$
    .subscribe(() => this.loadUnreadNotifications(true));

  this.notificationSubscription$ = this.signalR.notification$
    .pipe(filter(n => !!n))
    .subscribe((n) => {
      if (n!.id && this.notifications.some(x => x.id === n!.id)) return;

      const notification: HeaderNotification = {
        id: n!.id ?? 0,
        message: n!.message,
        createdAt: n!.createdAt ? this.parseNotificationDate(n!.createdAt) : new Date(),
        isRead: false,
        link: n!.link || '/pages/notifications-list',
        type: n!.taskId ? 'task' : 'other',
        taskId: n!.taskId
      };
      this.toastr.info(n!.message, this.translationService.instant('nav.notifications.notification'));

      this.notifications = [notification, ...this.notifications];

      this.notificationCount =
        this.notifications.filter(x => !x.isRead).length;
    });
}

  private loadUnreadNotifications(replace = false): void {
    this.notificationService.getUnread().subscribe({
      next: (res) => {
        const unread = (res?.data ?? []).map((n: any) => ({
          id: n.id,
          message: n.message,
          createdAt: this.parseNotificationDate(n.createdDate ?? n.createdAt),
          isRead: false,
          link: n.link || '/pages/notifications-list',
          type: n.taskId ? 'task' : 'other',
          taskId: n.taskId ?? undefined
        } as HeaderNotification));

        // API is newest-first; keep that order (do not unshift in a loop — it reverses the list).
        if (replace || this.notifications.length === 0) {
          this.notifications = unread;
        } else {
          const existingIds = new Set(this.notifications.map(x => x.id));
          const incoming = unread.filter(n => !existingIds.has(n.id));
          this.notifications = [...incoming, ...this.notifications]
            .sort((a, b) => b.createdAt.getTime() - a.createdAt.getTime());
        }

        this.notificationCount =
          this.notifications.filter(x => !x.isRead).length;
      },
      error: (err) => {
        console.error('Error loading unread notifications:', err);
      }
    });
  }

  /** Server stores UTC; values without Z must be treated as UTC. */
  private parseNotificationDate(value: string | Date | null | undefined): Date {
    if (!value) return new Date();
    if (value instanceof Date) return value;

    const raw = String(value).trim();
    if (!raw) return new Date();

    if (/[zZ]|[+-]\d{2}:?\d{2}$/.test(raw)) {
      return new Date(raw);
    }

    return new Date(raw.endsWith('Z') ? raw : `${raw}Z`);
  }

  private clearNotificationsUi() {
    this.notifications = [];
    this.notificationCount = 0;
    this.isNotifyEmpty = true;
    this.signalR.clearPendingNotification();
    this.toastr.clear();
  }
  removeNotification(notification: HeaderNotification, event?: Event) {
    event?.stopPropagation();

    const removeLocally = () => {
      this.notifications = this.notifications.filter(n => n.id !== notification.id);
      this.notificationCount = this.notifications.filter(x => !x.isRead).length;
    };

    if (!notification.isRead && notification.id > 0) {
      this.notificationService.markAsRead(notification.id).subscribe({
        next: () => {
          notification.isRead = true;
          removeLocally();
        },
        error: () => {
          removeLocally();
        }
      });
      return;
    }

    removeLocally();
  }

  ngOnDestroy() {
    this.clearNotificationsUi();
    this.notificationSubscription$?.unsubscribe();
    this.unreadChangedSubscription$?.unsubscribe();
    if (this.menuitemsSubscribe$) {
      this.menuitemsSubscribe$.unsubscribe();
    }
  }

  Search(searchText: string) {
    if (!searchText) return this.menuItems = [];
    // items array which stores the elements
    const items: Item[] = [];
    // Converting the text to lower case by using toLowerCase() and trim() used to remove the spaces from starting and ending
    searchText = searchText.toLowerCase().trim();
    this.items.filter((menuItems: MenuItem) => {
      // checking whether menuItems having title property, if there was no title property it will return
      if (!menuItems?.title) return false;
      //  checking wheteher menuitems type is text or string and checking the titles of menuitems
      if (menuItems.type === 'link' && menuItems.title.toLowerCase().includes(searchText)) {
        // Converting the menuitems title to lowercase and checking whether title is starting with same text of searchText
        if (menuItems.title.toLowerCase().startsWith(searchText)) { // If you want to get all the data with matching to letter entered remove this line(condition and leave items.push(menuItems))
          // If both are matching then the code is pushed to items array
          items.push(menuItems as Item);
        }
      }
      //  checking whether the menuItems having children property or not if there was no children the return
      if (!menuItems.children) return false;
      menuItems.children.filter((subItems: MenuItem) => {
        if (!subItems?.title) return false;
        if (subItems.type === 'link' && subItems.title.toLowerCase().includes(searchText)) {
          if (subItems.title.toLowerCase().startsWith(searchText)) {         // If you want to get all the data with matching to letter entered remove this line(condition and leave items.push(subItems))
            items.push(subItems as Item);
          }

        }
        if (!subItems.children) return false;
        subItems.children.filter((subSubItems: MenuItem) => {
          if (subSubItems.title?.toLowerCase().includes(searchText)) {
            if (subSubItems.title.toLowerCase().startsWith(searchText)) { // If you want to get all the data with matching to letter entered remove this line(condition and leave items.push(subSubItems))
              items.push(subSubItems as Item);

            }
          }
        });
        return true;
      });
      return this.menuItems = items;
    });
    // Used to show the No search result found box if the length of the items is 0
    if (!items.length) {
      this.SearchResultEmpty = true;
    }
    else {
      this.SearchResultEmpty = false;
    }
    return true;
  }
  SearchModal(SearchModal: any) {
    this.modalService.open(SearchModal);
  }
  //  Used to clear previous search result
  clearSearch() {
    const headerSearch = document.querySelector('.header-search');
    if (headerSearch) {
      headerSearch.classList.remove('searchdrop');
    }
    this.text = '';
    this.menuItems = [];
    this.SearchResultEmpty = false;
    return this.text, this.menuItems;

  }
  SearchHeader() {
    document
      .querySelector('.header-search')
      ?.classList.add('searchdrop');
  }

  Logout() {
    this.clearNotificationsUi();
    this.notificationSubscription$?.unsubscribe();
    this.authService.logout();
  }

handleNotificationClick(notification: HeaderNotification, event: Event) {
  event.stopPropagation();

  if (!notification.isRead && notification.id > 0) {
    this.notificationService.markAsRead(notification.id).subscribe({
      next: () => {
        notification.isRead = true;
        this.notificationCount = this.notifications.filter(x => !x.isRead).length;
      }
    });
  }

  if (notification.taskId != null) {
    this.onEdit(notification.taskId);
  }
}

rows: { taskId: number; [key: string]: any }[] = [];

onEdit(id: number) {
  console.log('Editing task ID:', id);
 if (id == null) return;

  const task = this.rows.find((x: { taskId: number }) => x.taskId === id);
  console.log('Task object:', task);

  const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      windowClass: 'task-details-modal',
      backdrop: 'static',
      scrollable: true
  });

  modalRef.componentInstance.taskId = id;
  modalRef.componentInstance.readonly = true;
}

}

