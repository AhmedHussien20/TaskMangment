import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';

import { GenericTableComponent, TableColumn } from '../../../shared/components/generic-table/generic-table.component';
import { TranslateModule } from '@ngx-translate/core';


import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { DashboardService } from 'app/core/services/dashboar.service';
import { TodayCommentTaskDto } from 'app/core/models/dashboard/dashboard.model';
import { TaskDetailsShellComponent } from 'app/components/tasks/task-details/task-details-shell/task-details-shell.component';

@Component({
  selector: 'app-task-today-comment',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    GenericTableComponent,
    TranslateModule
  ],
  templateUrl: './task-today-comment.component.html',
  styleUrls: ['./task-today-comment.component.scss']
})
export class TaskTodayCommentComponent implements OnInit {
  title = 'DASHBOARD.NOT_COMMENTED_TODAY';

  columns: TableColumn[] = [
    { key: 'taskId', label: 'TASK.ID' },
    { key: 'title', label: 'TASK.TITLE' },
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
        { key: 'assignedBy', label: 'TASK.ASSIGNED_BY' },
    { key: 'dueDate', label: 'TASK.DUE_DATE', type: 'date' }
  ];

  rows: TodayCommentTaskDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 5;

  searchCriteria: SearchCriteria = {
    searchKey: '',
    pageIndex: this.page,
    pageSize: this.entries,
    sortColumn: 'DueDate',
    sortDirection: 'ASC',
    filterTypes: {
      searchKey: 'text'
    }
  };

  labels = {
    searchKey: 'TASK.SEARCH'
  };

  isLoading = false;

  constructor(private dashboardService: DashboardService,private modalService: NgbModal) {}

  ngOnInit(): void {
    this.loadData();
  }

 loadData() {
  this.isLoading = true;

  this.dashboardService.getTasksNotCommentToday(this.searchCriteria)
    .subscribe({
      next: (res) => {
        const data = res.data;
        this.rows = data.data.map(x => ({
          ...x,
          id: x.taskId  
        }));
        this.totalItems = data.totalCount;
        this.page = data.pageIndex;
        this.entries = data.pageSize;
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

  checkRowClickable(item: any): boolean {
  return true;
  
}

onEdit(id: number) {
  console.log('Editing task ID:', id);
  
  const task = this.rows.find(x => x.taskId === id);
  console.log('Task object:', task);

  const modalRef = this.modalService.open(TaskDetailsShellComponent, {
      size: 'xl',
      backdrop: 'static',
      scrollable: true
    });

    modalRef.componentInstance.taskId = id;
    modalRef.componentInstance.readonly = true;
  }

}



