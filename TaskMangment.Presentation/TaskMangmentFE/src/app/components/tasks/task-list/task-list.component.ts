import { TaskGet } from "app/core/models/task/task";
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TaskCreateUpdateComponent } from "../task-create-update/task-create-update.component";
import { TaskService } from "app/core/services/task.service";
import { SearchCriteria } from "app/models/search-criteria.model";
import { TaskDetailsShellComponent } from "../task-details/task-details-shell/task-details-shell.component";
import Swal from 'sweetalert2';
import { ToastrService } from "ngx-toastr";


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
    TaskDetailsShellComponent,
  ],
  templateUrl: './task-list.component.html'
})
export class TaskListComponent implements OnInit {

  title = 'TASK.LIST_TITLE';
  breadcrumbs = ['HOME', 'TASKS'];
  activeitem = 'TASK.LIST_TITLE';
  isLoading = false;
  columns = [
    { key: 'id', label: 'TASK.ID' },
    { key: 'title', label: 'TASK.TITLE' },
    { key: 'assignedByName', label: 'TASK.ASSIGNED_BY' },
    {
      key: 'priorityText',
      label: 'TASK.PRIORITY'
      //,
      // type: 'badge' as const,
      // badgeMap: {
      //   Low:    { text: 'TASK.PRIORITY_LOW', class: 'bg-success' },
      //   Medium: { text: 'TASK.PRIORITY_MEDIUM', class: 'bg-warning' },
      //   High:   { text: 'TASK.PRIORITY_HIGH', class: 'bg-danger' }
      // }
    },

    {
      key: 'statusText',
      label: 'TASK.STATUS',
      type: 'badge' as const,
      badgeMap: {
        New: { text: 'TASK.STATUS_NEW', class: 'bg-secondary' },
        InProgress: { text: 'TASK.STATUS_IN_PROGRESS', class: 'bg-info' },
        Closed: { text: 'TASK.STATUS_CLOSED', class: 'bg-success' },
        Archived: { text: 'TASK.STATUS_ARCHIVED', class: 'bg-dark' }
      },
    },
    { key: 'dueDate', label: 'TASK.DUE_DATE' }
  ];

  rows: TaskGet[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;


  searchCriteria: SearchCriteria = {
    searchKey: '',
    pageIndex: this.page,
    pageSize: this.entries,
    sortColumn: 'Id',
    sortDirection: 'ASC',
    filterTypes: {
      searchKey: 'text',
    }
  };

  labels = {
    searchKey: 'TASK.searchKey'
  };
  isEdit = false;
  selectedTaskId: number | null = null;

  constructor(
    private taskService: TaskService,
    private modalService: NgbModal,
      private toastr: ToastrService,
    private translate: TranslateService

  ) { }

  ngOnInit() {
    this.loadData();
  }

  loadData() {
    this.taskService.getAll(this.searchCriteria).subscribe({
      next: (res: any) => {
        this.rows = res.data.data;
        this.totalItems = res.data.totalCount;
        this.page = res.data.pageIndex;
        this.entries = res.data.pageSize;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
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
    this.isEdit = false;
    this.selectedTaskId = null;
    this.modalService.open(modal, { size: 'lg', centered: true });
  }

  openEdit(id: number, modal: any) {
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
    title: this.translate.instant('TASK.CONFIRM_DELETE_TITLE'),
    text: this.translate.instant('TASK.CONFIRM_DELETE_TEXT'),
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: this.translate.instant('TASK.DELETE_BUTTON'),
    cancelButtonText: this.translate.instant('TASK.CANCEL_BUTTON'),
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

      this.toastr.error(
        this.translate.instant('TASK.DELETE_FAILED'),
        undefined,
        { timeOut: 3000 }
      );
    }
  });
}


}
