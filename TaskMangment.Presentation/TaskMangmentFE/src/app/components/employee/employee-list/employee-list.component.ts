import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { GenericTableComponent, TableColumn } from '../../../shared/components/generic-table/generic-table.component';
import { EmployeeService } from 'app/core/services/employee.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Employee } from 'app/core/models/employee/employee';
import { EmployeeCreateUpdateComponent } from '../employee-create-update/employee-create-update.component';
import Swal from 'sweetalert2';
import { ToastrService } from 'ngx-toastr';
import { BranchService } from 'app/core/services/branch.service';


@Component({
  selector: 'app-employee-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule,
    EmployeeCreateUpdateComponent,
    NgbModalModule
  ],
  templateUrl: './employee-list.component.html',
  styleUrls: ['./employee-list.component.scss']
})
export class EmployeeListComponent implements OnInit {

  title = 'EMPLOYEE.LIST_TITLE';
  activeitem = 'EMPLOYEE.LIST_TITLE';
  breadcrumbs = [
    'MENU.HOME',
    'MENU.EMPLOYEES',
    'EMPLOYEE.LIST_TITLE'
  ];
extraFilters: any = {};
  branchOptions: { id: number; name: string }[] = [];
  // table columns
  columns: TableColumn[] = [
    { key: 'id', label: 'EMPLOYEE.ID' },
    { key: 'fullName', label: 'EMPLOYEE.NAME' },
    { key: 'branchName', label: 'EMPLOYEE.BRANCH' },
    { key: 'email', label: 'EMPLOYEE.EMAIL' },
    { key: 'mobile', label: 'EMPLOYEE.MOBILE' },
    { key: 'roles', label: 'EMPLOYEE.ROLES' },
    { key: 'lastLoginDate', label: 'EMPLOYEE.LAST_LOGIN', type: 'dateTime' }
  ];

  rows: Employee[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  searchCriteria: SearchCriteria = {
    searchKey: '',
    branchId: null as any,
    pageIndex: this.page,
    pageSize: this.entries,
    sortColumn: 'Id',
    sortDirection: 'DESC',
    filterTypes: {
      searchKey: 'text',
      branchId: 'dropdown',
    }
  };

  labels = {
    searchKey: 'EMPLOYEE.searchKey',
    branchId: 'EMPLOYEE.BRANCH'
  };

  isLoading = false;

  selectedEmployeeId: number | null = null;
  isEdit = false;

  constructor(
    private employeeService: EmployeeService,
    private modalService: NgbModal,
    private translate: TranslateService,
    private toastr: ToastrService,
    private branchService: BranchService
  ) { }

  ngOnInit(): void {
     this.branchOptions = [
    { id: 1, name: 'Cairo' },
    { id: 2, name: 'Alex' },
    { id: 3, name: 'Giza' },
  ];
  this.extraFilters = {
  branchId: this.branchOptions
};
    this.loadData();
    //this.loadBranches();
  }

  loadData() {
    this.isLoading = true;
    this.employeeService.getAll(this.searchCriteria).subscribe({
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


  loadBranches() {
    const req = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 500,
      sortColumn: 'Id',
      sortDirection: 'DESC'
    };

    this.branchService.getAll(req).subscribe(res => {
      const list = res.data.data;
      this.branchOptions = list.map((b: any) => ({ id: b.id, name: b.name }));
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
    this.selectedEmployeeId = null;
    this.key++; // force rebuild
    this.open(modal);
  }

  openEdit(id: number, modal: any) {
    this.isEdit = true;
    this.selectedEmployeeId = id;
    this.key++;
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

  confirmDelete(empId: number) {
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
        this.deleteEmployee(empId);
      }
    });
  }

  deleteEmployee(empId: number) {
    this.isLoading = true;

    this.employeeService.delete(empId).subscribe({
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