import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';

import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';

import { DepartmentService } from 'app/core/services/department.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { DepartmentGetDto } from 'app/core/models/department/department';

import { DepartmentCreateUpdateComponent } from '../department-create-update/department-create-update.component';

import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-department-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule,
    DepartmentCreateUpdateComponent,
    NgbModalModule
  ],
  templateUrl: './department-list.component.html',
  styleUrls: ['./department-list.component.scss']
})
export class DepartmentListComponent implements OnInit {

  title = 'DEPARTMENT.LIST_TITLE';
  breadcrumbs = ['HOME', 'DEPARTMENTS'];
  activeitem = 'DEPARTMENT.LIST_TITLE';

  columns = [
    { key: 'id', label: 'DEPARTMENT.ID' },
    { key: 'name', label: 'DEPARTMENT.NAME' },
    { key: 'branchName', label: 'DEPARTMENT.BRANCH' },
    { key: 'areaName', label: 'DEPARTMENT.AREA' },
    { key: 'managerName', label: 'DEPARTMENT.MANAGER' },
    { key: 'employeeCount', label: 'DEPARTMENT.EMPLOYEE_COUNT' },
  ];

  rows: DepartmentGetDto[] = [];
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
    searchKey: 'DEPARTMENT.SEARCH'
  };

  isLoading = false;

  selectedDeptId: number | null = null;
  isEdit = false;

  constructor(
    private deptService: DepartmentService,
    private modalService: NgbModal
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.deptService.getAll(this.searchCriteria).subscribe({
      next: (res: any) => {
        this.rows = res.data.data;
        this.totalItems = res.data.totalCount;
        this.page = res.data.pageIndex;
        this.entries = res.data.pageSize;
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
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
    this.selectedDeptId = null;
    this.open(modal);
  }

  openEdit(id: number, modal: any) {
    this.isEdit = true;
    this.selectedDeptId = id;
    this.open(modal);
  }

  open(content: any) {
    this.modalService.open(content, {
      centered: true,
      backdrop: true,
      size: 'lg',
      windowClass: 'effect-scale'
    });
  }

  onFormSubmitted() {
    this.modalService.dismissAll();
    this.loadData();
  }
}
