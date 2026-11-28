import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { ColumnType, GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { LeaveTypeService } from 'app/core/services/leave-type.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { LeaveTypeCreateUpdateComponent } from '../leave-type-create-update/leave-type-create-update.component';
import Swal from 'sweetalert2';
import { LeaveTypeGetDto } from 'app/core/models/leave/leave-type.model';
import { AuthService } from 'app/core/services/auth.service';
import { Permissions } from 'app/core/constants/permissions';

@Component({
  selector: 'app-leave-type',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule,
    LeaveTypeCreateUpdateComponent
  ],
  templateUrl: './leave-type.component.html',
  styleUrls: ['./leave-type.component.scss']
})
export class LeaveTypeComponent implements OnInit {

  title = 'LEAVE_TYPE.title';
  activeitem = 'LEAVE_TYPE.title';
  breadcrumbs = ['MENU.HOME', 'MENU.LEAVES', 'LEAVE_TYPE.title'];

  canCreate: boolean = false;
  canEdit: boolean = false;
  canDelete: boolean = false;


  columns: TableColumn[] = [
  { key: 'id', label: 'LEAVE_TYPE.ID' },
  { key: 'nameAr', label: 'LEAVE_TYPE.NAME_AR' },
  { key: 'nameEn', label: 'LEAVE_TYPE.NAME_EN' },
  { key: 'isPaid', label: 'LEAVE_TYPE.IS_PAID', type: 'badge', badgeMap: {
      true: { text: 'LEAVE_TYPE.PAID_YES', class: 'bg-success' },
      false: { text: 'LEAVE_TYPE.PAID_NO', class: 'bg-secondary' }
  }},
  { key: 'maxDaysPerYear', label: 'LEAVE_TYPE.MAX_DAYS' },
];


  rows: LeaveTypeGetDto[] = [];
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

  labels = { searchKey: 'LEAVE_TYPE.SEARCH' };
  isLoading = false;

  // Modal
  modal: any;
  selectedLeaveTypeId: number | null = null;
  isEdit = false;

  constructor(
    private router: Router,
    private leaveTypeService: LeaveTypeService,
    private modalService: NgbModal,
    private translate: TranslateService,
    private toastr: ToastrService,
    private auth:AuthService
  ) { }

  ngOnInit(): void {
    this.canCreate = this.auth.hasPermission(Permissions.ASSIGN_ROLE);
    this.canEdit = this.auth.hasPermission(Permissions.ASSIGN_ROLE);
    this.canDelete = this.auth.hasPermission(Permissions.ASSIGN_ROLE);
    this.loadData();
  }

  // Load Data
  loadData() {
  this.isLoading = true;
  this.leaveTypeService.getAll().subscribe({
    next: (res: any) => {
      this.rows = res.data;        
      this.totalItems = res.data.length; 
      this.page = 1;
      this.entries = 10;           
      this.isLoading = false;
    },
    error: () => this.isLoading = false
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

  // Modal Handlers
  openAdd(modal: any) {
    this.isEdit = false;
    this.selectedLeaveTypeId = null;
    this.openModal(modal);
  }

  openEdit(id: number, modal: any) {
    this.isEdit = true;
    this.selectedLeaveTypeId = id;
    this.openModal(modal);
  }

  openModal(content: any) {
    this.modalService.open(content, { centered: true, size: 'lg', backdrop: true });
  }

  onFormSubmitted() {
    this.modalService.dismissAll();
    this.loadData();
  }

  // Delete
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
      if (result.isConfirmed) this.deleteLeaveType(id);
    });
  }

  deleteLeaveType(id: number) {
    this.isLoading = true;
    this.leaveTypeService.delete(id).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('COMMON.DELETE_SUCCESS'));
        this.loadData();
        this.isLoading = false;
      },
      error: () => this.isLoading = false
    });
  }

}
