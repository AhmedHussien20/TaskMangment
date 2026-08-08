import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';

import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { EmployeeTypeService } from 'app/core/services/employee-type.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { EmployeeTypeGetDto } from 'app/core/models/employee/employee-type.model';
import { EmployeeTypeCreateUpdateComponent } from '../employee-type-create-update/employee-type-create-update.component';
import { AuthService } from 'app/core/services/auth.service';
import { Permissions } from 'app/core/constants/permissions';

@Component({
  selector: 'app-employee-type-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    NgbModalModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule,
    EmployeeTypeCreateUpdateComponent
  ],
  templateUrl: './employee-type-list.component.html'
})
export class EmployeeTypeListComponent implements OnInit {

  title = 'EMPLOYEE_TYPE.TITLE';
  activeitem = 'EMPLOYEE_TYPE.TITLE';
  breadcrumbs = ['MENU.HOME', 'MENU.EMPLOYEES', 'EMPLOYEE_TYPE.TITLE'];

  canCreate = false;
  canEdit = false;
  canDelete = false;

  columns: TableColumn[] = [
    { key: 'code', label: 'EMPLOYEE_TYPE.CODE' },
    { key: 'nameEn', label: 'EMPLOYEE_TYPE.NAME_EN' },
    { key: 'nameAr', label: 'EMPLOYEE_TYPE.NAME_AR' },
    {
      key: 'seesAllTypesInBranchScope',
      label: 'EMPLOYEE_TYPE.SEES_ALL_TYPES',
      type: 'badge',
      badgeMap: {
        true: { text: 'LEAVE_TYPE.PAID_YES', class: 'bg-success' },
        false: { text: 'LEAVE_TYPE.PAID_NO', class: 'bg-secondary' }
      }
    },
    { key: 'employeeCount', label: 'EMPLOYEE_TYPE.EMPLOYEE_COUNT' }
  ];

  rows: EmployeeTypeGetDto[] = [];
  totalItems = 0;
  page = 1;
  entries = 10;

  searchCriteria: SearchCriteria = {
    searchKey: '',
    pageIndex: this.page,
    pageSize: this.entries,
    sortColumn: 'Id',
    sortDirection: 'DESC',
    filterTypes: { searchKey: 'text' }
  };

  labels = { searchKey: 'EMPLOYEE_TYPE.SEARCH' };
  isLoading = false;

  selectedEmployeeTypeId: number | null = null;
  isEdit = false;

  constructor(
    private employeeTypeService: EmployeeTypeService,
    private modalService: NgbModal,
    private translate: TranslateService,
    private toastr: ToastrService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    this.canCreate = this.auth.hasPermission(Permissions.CREATE_EMPLOYEE_TYPE);
    this.canEdit = this.auth.hasPermission(Permissions.UPDATE_EMPLOYEE_TYPE);
    this.canDelete = this.auth.hasPermission(Permissions.DELETE_EMPLOYEE_TYPE);
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.employeeTypeService.getAll(this.searchCriteria).subscribe({
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
    this.searchCriteria = { ...this.searchCriteria, ...filters, pageIndex: 1 };
    this.page = 1;
    this.loadData();
  }

  openAdd(modal: any) {
    this.isEdit = false;
    this.selectedEmployeeTypeId = null;
    this.openModal(modal);
  }

  openEdit(id: number, modal: any) {
    this.isEdit = true;
    this.selectedEmployeeTypeId = id;
    this.openModal(modal);
  }

  openModal(content: any) {
    this.modalService.open(content, { centered: true, size: 'lg', backdrop: true });
  }

  onFormSubmitted() {
    this.modalService.dismissAll();
    this.loadData();
  }

  confirmDelete(id: number) {
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
      if (result.isConfirmed) this.deleteEmployeeType(id);
    });
  }

  deleteEmployeeType(id: number) {
    this.isLoading = true;
    this.employeeTypeService.delete(id).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('COMMON.DELETE_SUCCESS'));
        this.loadData();
        this.isLoading = false;
      },
      error: () => { this.isLoading = false; }
    });
  }
}
