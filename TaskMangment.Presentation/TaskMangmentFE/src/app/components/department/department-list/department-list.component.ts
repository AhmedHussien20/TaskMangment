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

import { TranslateModule, TranslateService } from '@ngx-translate/core';
import Swal from 'sweetalert2';
import { ToastrService } from 'ngx-toastr';

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
  activeitem = 'DEPARTMENT.LIST_TITLE';
  breadcrumbs = [
  'MENU.HOME',
  'MENU.ORGANIZATION_STRUCTURE',
  'DEPARTMENT.LIST_TITLE'
];

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
    sortDirection: 'DESC',
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
    private modalService: NgbModal,
    private translate: TranslateService,
    private toastr: ToastrService
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
key = 0;
  openAdd(modal: any) {
    this.isEdit = false;
    this.selectedDeptId = null;
    this.key++; 
    this.open(modal);
  }

  openEdit(id: number, modal: any) {
    this.isEdit = true;
    this.selectedDeptId = id;
    this.key++;
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

   confirmDelete(deptId: number) {
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
          this.deleteDepartment(deptId);
        }
      });
    }
    
    deleteDepartment(deptId: number) {
      this.isLoading = true;

      this.deptService.delete(deptId).subscribe({
        next: () => {
        this.toastr.success(this.translate.instant('COMMON.DELETE_SUCCESS'));
    
    
          this.isLoading = false;
          this.loadData();
        },
        error: () => {
          this.isLoading = false;
        }
      });
    }
    
}
