import { TaskGet } from "app/core/models/task/task";
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
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
import { SpkDashboardComponent } from "app/@spk/reusable-dashboard/spk-dashboard/spk-dashboard.component";
import { EmployeeService } from "app/core/services/employee.service";


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
    SpkDashboardComponent
    // TaskDetailsShellComponent,
  ],
  templateUrl: './task-list.component.html'
})
export class TaskListComponent implements OnInit {

  canCreate = false;
  canEdit = false;
  canDelete = false;
  showEmployeeFilter = false;

  summary!: {
    myTasks: number;
    createdByMe: number;
    inProgressTasks: number;
    newTasks: number;
    archiveTasks: number;
   // autoClose : number;
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
        AutoClose: {text: 'TASK.STATUS_AUTOCLOSE',class: 'bg-warning' },
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
    { id: 5, name:'TASK.STATUS_AUTOCLOSE'},
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
      statusId: 'dropdown'
    }
  };


  labels = {
    searchKey: 'TASK.searchKey',
    employeeIds: 'TASK.employee',
    statusId: 'TASK.STATUS'
  };
  isEdit = false;
  selectedTaskId: number | null = null;

  constructor(
    private taskService: TaskService,
    private modalService: NgbModal,
    private toastr: ToastrService,
    private translate: TranslateService,
    private auth: AuthService,
    private employeeService: EmployeeService

  ) { }

  ngOnInit() {
    const roleLevel = this.auth.getRoleLevel();
    this.showEmployeeFilter = roleLevel >= 50;
    if (this.showEmployeeFilter) {
      this.loadEmployees();
    }

    this.status = this.statusOptions;
    this.canCreate = roleLevel >= 50;
    this.canEdit = roleLevel >= 70;
    this.canDelete = roleLevel >= 70;
    this.loadData();
  }

  loadEmployees() {
    const request = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 1000,
      sortColumn: 'Id',
      sortDirection: 'DESC'
    };

    this.employeeService.getAll(request).subscribe(res => {
      console.log(res);
      this.employees = res.data.data.map((e: any) => ({
        id: e.id,
        name: e.fullName
      }));
    });
  }

  loadData() {
    this.isLoading = true;
    this.taskService.getAll(this.searchCriteria).subscribe({
      next: (res: any) => {

        const payload = res;
        const pageData = payload.data;

        this.rows = pageData.data ?? [];
        this.totalItems = pageData.totalCount ?? 0;
        this.page = pageData.pageIndex ?? 1;
        this.entries = pageData.pageSize ?? 10;

        this.summary = pageData.summary;
        console.log(this.summary);
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
      // {
      //   title: 'TASK.CREATED_BY_ME',
      //   value: this.summary.createdByMe,
      //   svg: `
      //   <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24"
      //        viewBox="0 0 24 24" fill="none" stroke="currentColor"
      //        stroke-width="2" stroke-linecap="round" stroke-linejoin="round"
      //        class="feather feather-user-check text-success">
      //     <path d="M16 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
      //     <circle cx="8.5" cy="7" r="4"></circle>
      //     <polyline points="17 11 19 13 23 9"></polyline>
      //   </svg>
      // `
      // },
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
    this.searchCriteria = {
      ...this.searchCriteria,
      ...filters,
      pageIndex: 1
    };
    this.page = 1;
    this.loadData();
  }

  openAdd(modal: any) {
    if (!this.canCreate) return;
    this.isEdit = false;
    this.selectedTaskId = null;
    this.modalService.open(modal, { size: 'lg', centered: true });
  }


  openEdit(id: number, modal: any) {
    if (!this.canEdit) return;
    this.isEdit = true;
    this.selectedTaskId = id;
    this.modalService.open(modal, { size: 'lg', centered: true });
  }

  onFormSubmitted() {
    this.modalService.dismissAll();
    this.loadData();
  }


  openDetails(taskId: number) {
    const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      backdrop: 'static',
      scrollable: true
    });

    modalRef.componentInstance.taskId = taskId;
    modalRef.componentInstance.readonly = true;
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


}
