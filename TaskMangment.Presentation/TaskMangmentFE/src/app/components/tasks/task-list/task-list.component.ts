import { TaskGet } from "app/core/models/task/task";
import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { NgbModal, NgbModalModule } from '@ng-bootstrap/ng-bootstrap';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TaskCreateUpdateComponent } from "../task-create-update/task-create-update.component";
import { TaskService } from "app/core/services/task.service";
import { SearchCriteria } from "app/models/search-criteria.model";
import { TaskDetailsShellComponent } from "../task-details/task-details-shell/task-details-shell.component";
import Swal from 'sweetalert2';
import { ToastrService } from "ngx-toastr";
import { AuthService } from "app/core/services/auth.service";
import { Permissions } from "app/core/constants/permissions";
import { SpkDashboardComponent } from "app/@spk/reusable-dashboard/spk-dashboard/spk-dashboard.component";
import { EmployeeService } from "app/core/services/employee.service";
import { TasksEmployeeDeepSearchComponent } from "../tasks-employee-deep-search/tasks-employee-deep-search.component";
import { DatePickerComponent } from "app/components/date-picker/date-picker.component";

@Component({
  selector: 'app-task-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    GenericTableComponent,
    PageHeaderComponent,
    NgbModalModule,
    TaskCreateUpdateComponent,
    SpkDashboardComponent,
    TasksEmployeeDeepSearchComponent
  ],
  templateUrl: './task-list.component.html'
})
export class TaskListComponent implements OnInit {
  IsClosed = true;
  canArchive = false;
  TurnToArchive = true;
@ViewChild(TasksEmployeeDeepSearchComponent)
deepSearchComp?: TasksEmployeeDeepSearchComponent;
  canCreate = false;
  canEdit = false;
  canDelete = false;
  canShowExtraTasks= false
  showEmployeeFilter = false;
  showTaskScopeFilter = false;
  deepSearchInitial: any = null;
  summary!: {
    myTasks: number;
    createdByMe: number;
    inProgressTasks: number;
    newTasks: number;
    archiveTasks: number;
  };

  employees: { id: number; name: string }[] = [];
  status: { id: number; name: string }[] = [];

  title = 'TASK.LIST_TITLE';
  activeitem = 'TASK.LIST_TITLE';
  breadcrumbs = [
    'MENU.HOME',
    'MENU.EMPLOYMENT',
    'TASK.LIST_TITLE'
  ];

  isLoading = false;

  columns: TableColumn[] = [
    { key: 'id', label: 'TASK.ID' },
    { key: 'title', label: 'TASK.TITLE' },
    { key: 'assignedByName', label: 'TASK.ASSIGNED_BY' },
    {
      key: 'assignEmployee',
      label: 'TASK.ASSIGNED_TO',
      type: 'assignees',
      displayField: 'name'
    },
    {
      key: 'priorityText',
      label: 'TASK.PRIORITY',
      type: 'badge',
      badgeMap: {
        Low: { text: 'TASK.PRIORITY_LOW', class: 'bg-success' },
        Medium: { text: 'TASK.PRIORITY_MEDIUM', class: 'bg-warning' },
        High: { text: 'TASK.PRIORITY_HIGH', class: 'bg-danger' }
      }
    },
    {
      key: 'statusText',
      label: 'TASK.STATUS',
      type: 'badge',
      badgeMap: {
        New: { text: 'TASK.STATUS_NEW', class: 'bg-secondary' },
        InProgress: { text: 'TASK.STATUS_IN_PROGRESS', class: 'bg-info' },
        Closed: { text: 'TASK.STATUS_CLOSED', class: 'bg-success' },
        Archived: { text: 'TASK.STATUS_ARCHIVED', class: 'bg-dark' },
        AutoClose: { text: 'TASK.STATUS_AUTOCLOSE', class: 'bg-warning' },
      }
    },
    { key: 'dueDate', label: 'TASK.DUE_DATE', type: 'date' }
  ];

  rows: TaskGet[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  statusOptions = [
    { id: 1, name: 'TASK.STATUS_NEW' },
    { id: 2, name: 'TASK.STATUS_IN_PROGRESS' },
    { id: 3, name: 'TASK.STATUS_CLOSED' },
    { id: 4, name: 'TASK.STATUS_ARCHIVED' },
    { id: 5, name: 'TASK.STATUS_AUTOCLOSE' },
  ];

  searchCriteria: SearchCriteria = {
    searchKey: '',
    employeeIds: [] as number[],
    statusId: null,
    pageIndex: this.page,
    pageSize: this.entries,
    sortColumn: 'Id',
    sortDirection: 'DESC',
    filterTypes: {
      searchKey: 'text',
      employeeIds: 'dropdown',
      statusId: 'dropdown',
    }
  };

  labels = {
    searchKey: 'TASK.searchKey',
    employeeIds: 'TASK.employee',
    statusId: 'TASK.STATUS',
    viewScopedTasks: 'TASK.TASK_SCOPE'
  };

  taskScopeFilterOptions: Record<string, { id: string; name: string }[]> = {
    viewScopedTasks: [
      { id: 'my', name: 'TASK.MY_TASKS' },
      { id: 'scoped', name: 'TASK.ALL_TASKS' }
    ]
  };

  isEdit = false;
  selectedTaskId: number | null = null;
  canCopy = false;
  isCopyMode = false;
  

  constructor(
    private taskService: TaskService,
    private modalService: NgbModal,
    private toastr: ToastrService,
    private translate: TranslateService,
    private auth: AuthService,
    private employeeService: EmployeeService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    this.showEmployeeFilter = this.auth.hasAnyPermission(
      Permissions.VIEW_SCOPED_TASKS,
      Permissions.VIEW_COMPANY_TASKS,
      Permissions.VIEW_EMPLOYEES
    );

    this.status = this.statusOptions;

    const user = this.auth.getUser() ?? this.auth.getCurrentUser();
    this.showTaskScopeFilter = !!user?.hasAccessScope;

    if (this.showTaskScopeFilter) {
      this.searchCriteria = {
        ...this.searchCriteria,
        viewScopedTasks: 'my',
        filterTypes: {
          viewScopedTasks: 'dropdown',
          searchKey: 'text',
          employeeIds: 'dropdown',
          statusId: 'dropdown',
        }
      };
    }

    this.canCreate = this.auth.hasPermission(Permissions.CREATE_TASK);
    // Show action columns when user may own tasks and/or has update/delete permission.
    this.canEdit =
      this.auth.hasPermission(Permissions.UPDATE_TASK) ||
      this.auth.hasPermission(Permissions.CREATE_TASK);
    this.canDelete =
      this.auth.hasPermission(Permissions.DELETE_TASK) ||
      this.auth.hasPermission(Permissions.CREATE_TASK);
    this.canCopy = this.auth.hasPermission(Permissions.CREATE_TASK);
    this.canArchive = this.auth.hasPermission(Permissions.ARCHIVE_TASK);
    this.canShowExtraTasks = this.auth.hasPermission(
      Permissions.VIEW_COMPANY_TASKS
    );

    this.loadData();
    this.openTaskFromQueryIfAny();
  }

  private openTaskFromQueryIfAny(): void {
    const taskId = Number(this.route.snapshot.queryParamMap.get('taskId'));
    if (Number.isNaN(taskId) || taskId <= 0) {
      return;
    }

    // Open as popup only; API getById enforces that the user can access this task.
    this.openDetails(taskId);
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: { taskId: null },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  
 loadData() {
  this.isLoading = true;

  this.taskService.getAll(this.searchCriteria).subscribe({
    next: (res: any) => {
      const pageData = res?.data;

      this.rows = (pageData?.data ?? []).map((r: any) => ({
        ...r,
        __archiveChecked: false
      }));

      this.selectedArchiveIds = [];

      this.totalItems = pageData?.totalCount ?? 0;
      this.page = pageData?.pageIndex ?? 1;
      this.entries = pageData?.pageSize ?? 10;
      this.summary = pageData?.summary;
      this.buildSummaryCards();

      this.isLoading = false;
    },
    error: () => {
      this.isLoading = false;
    }
  });
}


  cards: any[] = [];

  private buildSummaryCards() {
    if (!this.summary) {
      this.cards = [];
      return;
    }

    this.cards = [
      {
        title: 'TASK.MY_TASKS',
        value: this.summary.myTasks,
        svg: `
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
             viewBox="0 0 24 24" fill="none" stroke="currentColor"
             stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
             class="feather feather-list text-primary">
          <line x1="8" y1="6" x2="21" y2="6"></line>
          <line x1="8" y1="12" x2="21" y2="12"></line>
          <line x1="8" y1="18" x2="21" y2="18"></line>
          <line x1="3" y1="6" x2="3" y2="6"></line>
          <line x1="3" y1="12" x2="3" y2="12"></line>
          <line x1="3" y1="18" x2="3" y2="18"></line>
        </svg>
      `
      },
      {
        title: 'TASK.IN_PROGRESS_TASKS',
        value: this.summary.inProgressTasks,
        svg: `
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
             viewBox="0 0 24 24" fill="none" stroke="currentColor"
             stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
             class="feather feather-activity text-warning">
          <polyline points="22 12 18 12 15 21 9 3 6 12 2 12"></polyline>
        </svg>
      `
      },
      {
        title: 'TASK.NEW_TASKS',
        value: this.summary.newTasks,
        svg: `
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
             viewBox="0 0 24 24" fill="none" stroke="currentColor"
             stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
             class="feather feather-plus-circle text-info">
          <circle cx="12" cy="12" r="10"></circle>
          <line x1="12" y1="8" x2="12" y2="16"></line>
          <line x1="8" y1="12" x2="16" y2="12"></line>
        </svg>
      `
      },
      {
        title: 'TASK.ARCHIVED_TASKS',
        value: this.summary.archiveTasks,
        svg: `
              <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
                  viewBox="0 0 24 24" fill="none" stroke="currentColor"
                  stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
                  class="feather feather-archive text-dark">
                <polyline points="21 8 21 21 3 21 3 8"></polyline>
                <rect x="1" y="3" width="22" height="5"></rect>
                <line x1="10" y1="12" x2="14" y2="12"></line>
              </svg>
            `
      }
    ];
  }

  onPageChange(page: number) {
    this.page = page;
    this.searchCriteria.pageIndex = page;
    this.loadData();
  }

  onEntriesChange(entries: number) {
    this.entries = entries;
    this.searchCriteria.pageSize = entries;
    this.searchCriteria.pageIndex = 1;
    this.page = 1;
    this.loadData();
  }

  applyFilters(filters: any) {
    Object.assign(this.searchCriteria, filters, { pageIndex: 1 });
    this.page = 1;
    this.loadData();
  }

  openAdd(modal: any) {
  this.isEdit = false;
  this.isCopyMode = false;
  this.selectedTaskId = null;
  this.modalService.open(modal, { size: 'xl', centered: true, backdrop: 'static' });
}

openEdit(id: number, modal: any) {
  this.isEdit = true;
  this.isCopyMode = false;
  this.selectedTaskId = id;
  this.modalService.open(modal, { size: 'xl', centered: true, backdrop: 'static' });
}
openCopy(id: number, modal: any) {
    console.log('copy event:', id);
  this.selectedTaskId = id;
  this.isEdit = false;
  this.isCopyMode = true;
  this.modalService.open(modal, { size: 'xl', centered: true, backdrop: 'static' });
}


  onFormSubmitted() {
    this.modalService.dismissAll();
    this.loadData();
  }

 openDetails(taskId: number) {
  const task = this.rows.find(x => x.id === taskId);

  // Verify access before opening popup so users can't open tasks that aren't theirs.
  this.taskService.getById(taskId).subscribe({
    next: (res) => {
      const modalRef = this.modalService.open(TaskDetailsShellComponent, {
        size: 'xl',
        backdrop: 'static',
        scrollable: true,
        windowClass: 'task-details-modal'
      });

      modalRef.componentInstance.taskId = taskId;
      modalRef.componentInstance.readonly = true;
      modalRef.componentInstance.createdByMe =
        res?.data?.createdByMe ?? task?.createdByMe ?? false;
    },
    error: () => {
      this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
    }
  });
}

  confirmDelete(taskId: number) {
    Swal.fire({
      title: this.translate.instant('COMMON.CONFIRM_DELETE_TITLE'),
      text: this.translate.instant('COMMON.CONFIRM_DELETE_TEXT'),
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: this.translate.instant('COMMON.DELETE_BUTTON'),
      cancelButtonText: this.translate.instant('COMMON.CANCEL_BUTTON'),
      confirmButtonColor: '#d33',
      cancelButtonColor: '#6c757d'
    }).then((result) => {
      if (result.isConfirmed) {
        this.deleteTask(taskId);
      }
    });
  }

  deleteTask(taskId: number) {
    this.isLoading = true;

    this.taskService.delete(taskId).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('TASK.DELETE_SUCCESS'));
        this.isLoading = false;
        this.loadData();
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  /** Literal owner can edit; scope-only needs UPDATE_TASK. */
  disableEditRow = (row: TaskGet) => {
    const closed = row.status === 3 || row.status === 4 || row.status === 5;
    if (closed) return true;
    if (row.isCreatorOrAssigner) return false;
    return !(row.createdByMe && this.auth.hasPermission(Permissions.UPDATE_TASK));
  };

  /** Literal owner can delete; scope-only needs DELETE_TASK. */
  disableDeleteRow = (row: TaskGet) => {
    const closed = row.status === 3 || row.status === 4 || row.status === 5;
    if (closed) return true;
    if (row.isCreatorOrAssigner) return false;
    return !(row.createdByMe && this.auth.hasPermission(Permissions.DELETE_TASK));
  };

  disableCopyRow = (row: TaskGet): boolean => {
    if (row.isCreatorOrAssigner) return false;
    return !(row.createdByMe && this.auth.hasPermission(Permissions.CREATE_TASK));
  };
deepSearchTitleKey: string = 'TASK.DEEP_SEARCH';
extraMenuItems = [
  { label: 'TASK.OUTGOING_NEW', value: { direction: 2, statusId: 1 } },
  { label: 'TASK.OUTGOING_INPROGRESS', value: { direction: 2, statusId: 2 } },
  { label: 'TASK.OUTGOING_AUTOCLOSE',  value: { direction: 2, statusId: 5 } },
  { label: 'TASK.OUTGOING_ARCHIVED',   value: { direction: 2, statusId: 4 } },

  { divider: true, label: '', value: null },

  { label: 'TASK.INCOMING_NEW', value: { direction: 1, statusId: 1 } },
  { label: 'TASK.INCOMING_INPROGRESS', value: { direction: 1, statusId: 2 } },
  { label: 'TASK.INCOMING_AUTOCLOSE',  value: { direction: 1, statusId: 5 } },
  { label: 'TASK.INCOMING_ARCHIVED',   value: { direction: 1, statusId: 4 } },
];

 onExtraMenuSelect(item: any, modalTpl: any) {
  if (!item || item.divider) return;

  const preset = item.value ?? {};

  this.deepSearchTitleKey = item.label ?? 'TASK.DEEP_SEARCH';

  this.deepSearchInitial = {
  direction: preset.direction ?? null,
  statusId: preset.statusId ?? null,
  targetEmployeeId: (this.searchCriteria as any)['targetEmployeeId'] ?? null,
  priorityId: (this.searchCriteria as any)['priorityId'] ?? null,
  createdFrom: (this.searchCriteria as any)['createdFrom'] ?? null,
  createdTo: (this.searchCriteria as any)['createdTo'] ?? null,
  dueFrom: (this.searchCriteria as any)['dueFrom'] ?? null,
  dueTo: (this.searchCriteria as any)['dueTo'] ?? null,
  searchKey: this.searchCriteria.searchKey ?? ''
};
  this.modalService.open(modalTpl, { size: 'lg', centered: true });
}


  applyDeepSearch(filters: any, modal: any) {
    this.searchCriteria = {
      ...this.searchCriteria,
      ...filters,
      pageIndex: 1
    };

    this.page = 1;
    modal.close(filters);
    this.loadData();
  }

  selectedArchiveIds: number[] = [];
onCheckboxChange(e: { row: TaskGet; checked: boolean }) {
  const row = e.row;

  const allowed = row.status === 3 || row.status === 5;

  if (!allowed) {
    (row as any)['__archiveChecked'] = false;
    this.rows = [...this.rows];
    return;
  }

  (row as any)['__archiveChecked'] = e.checked;

  if (e.checked) {
    if (!this.selectedArchiveIds.includes(row.id)) this.selectedArchiveIds.push(row.id);
  } else {
    this.selectedArchiveIds = this.selectedArchiveIds.filter(x => x !== row.id);
  }
}

 archiveSelectedTasks() {
  const count = this.selectedArchiveIds.length;
  if (!count) return;

  Swal.fire({
    title: this.translate.instant('TASK.ARCHIVE_TASKS'),
    text: this.translate.instant('TASK.ARCHIVE_CONFIRM_TEXT', { count }),
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: this.translate.instant('TASK.CONFIRM_BUTTON') ,
    cancelButtonText: this.translate.instant('COMMON.CANCEL_BUTTON'),
    confirmButtonColor: '#d33',
    cancelButtonColor: '#6c757d'
  }).then((result) => {
    if (!result.isConfirmed) return;

    this.taskService.archiveClosed(this.selectedArchiveIds).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('TASK.ARCHIVE_SUCCESS'));
        this.selectedArchiveIds = [];
        this.rows = this.rows.map(r => ({ ...(r as any), __archiveChecked: false }));
        this.loadData();
      }
    });
  });
}


disableArchiveCheckbox = (row: TaskGet) =>
  !((row.status === 3 || row.status === 5) && row.createdByMe);

  clearTrigger = 0;

 onTableCleared() {
  
  this.deepSearchInitial = null;
  (this.searchCriteria as any).direction = null;
  (this.searchCriteria as any).targetEmployeeId = null;
  (this.searchCriteria as any).priorityId = null;
  (this.searchCriteria as any).createdFrom = null;
  (this.searchCriteria as any).createdTo = null;
  (this.searchCriteria as any).dueFrom = null;
  (this.searchCriteria as any).dueTo = null;
  (this.searchCriteria as any).searchKey = '';
  if (this.showTaskScopeFilter) {
    (this.searchCriteria as any).viewScopedTasks = 'my';
  }
this.clearTrigger++;
  this.loadData();
}


}
