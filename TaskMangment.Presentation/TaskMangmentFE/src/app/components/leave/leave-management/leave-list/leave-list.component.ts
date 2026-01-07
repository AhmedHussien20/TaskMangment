import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';

import { LeaveService } from 'app/core/services/leave.service';
import { LeaveGetDto } from 'app/core/models/leave/leave';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { AuthService } from 'app/core/services/auth.service';
import { LeaveCreateUpdateComponent } from '../leave-create-update/leave-create-update.component';

@Component({
  selector: 'app-leave-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbModalModule,
    NgbPaginationModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule,
    LeaveCreateUpdateComponent
  ],
  templateUrl: './leave-list.component.html',
  styleUrls: ['./leave-list.component.scss']
})
export class LeaveListComponent implements OnInit {

  canCreate = false;
  canEdit = false;
  canDelete = false;

  title = 'LEAVE.LIST_TITLE';
  activeitem = 'LEAVE.LIST_TITLE';
  breadcrumbs = [
    'MENU.HOME',
    'MENU.HR',
    'LEAVE.LIST_TITLE'
  ];

  columns: TableColumn[] = [
    { key: 'id', label: 'LEAVE.ID' },
    { key: 'employeeName', label: 'LEAVE.EMPLOYEE' },
    { key: 'leaveTypeName', label: 'LEAVE.TYPE' },
    { key: 'startDate', label: 'LEAVE.START_DATE' },
    { key: 'endDate', label: 'LEAVE.END_DATE' },
    { key: 'statusName', label: 'LEAVE.STATUS' }
  ];

  rows: LeaveGetDto[] = [];
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

  labels = { searchKey: 'LEAVE.SEARCH' };
  isLoading = false;

  // MODALS
  isEdit = false;
  selectedLeaveId: number | null = null;
  selectedLeave: LeaveGetDto | null = null;
@ViewChild('detailsModal') detailsModal: any;



  constructor(
    private leaveService: LeaveService,
    private modalService: NgbModal,
    private toastr: ToastrService,
    private translate: TranslateService,
    private auth: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    const roleLevel = this.auth.getRoleLevel();

    this.canCreate = roleLevel >= 50;
    this.canEdit = roleLevel >= 70;
    this.canDelete = roleLevel >= 70;

    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    this.leaveService.getMyRequests(this.searchCriteria).subscribe({
      next: (res: any) => {
        const pageData = res.data;

        this.rows = pageData.data ?? [];
        this.totalItems = pageData.totalCount ?? 0;
        this.page = pageData.pageIndex ?? 1;
        this.entries = pageData.pageSize ?? 10;

       

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
    if (!this.canCreate) return;
    this.isEdit = false;
    this.selectedLeaveId = null;
    this.modalService.open(modal, { size: 'lg', centered: true });
  }

  onFormSubmitted() {
    this.modalService.dismissAll();
    this.loadData();
  }
openDetailsModal(id: number) {
  this.isLoading = true;
  this.leaveService.getById(id).subscribe({
    next: (res: any) => {
      this.selectedLeave = res.data;
      this.isLoading = false;
      this.modalService.open(this.detailsModal, { size: 'md', centered: true });
    },
    error: () => { this.isLoading = false; }
  });
}

approveLeave(id: number, modal?: any) {
  this.leaveService.approve(id).subscribe(() => {
    this.toastr.success(this.translate.instant('LEAVE.APPROVED'));
    if (modal) modal.close();
    this.loadData();
  });
}

rejectLeave(id: number, modal?: any) {
  Swal.fire({
    title: this.translate.instant('LEAVE.REJECT_TITLE'),
    input: 'textarea',
    showCancelButton: true,
    confirmButtonText: this.translate.instant('COMMON.CONFIRM'),
    cancelButtonText: this.translate.instant('COMMON.CANCEL')
  }).then(result => {
    if (result.isConfirmed && result.value) {
      this.leaveService.reject(id, result.value).subscribe(() => {
        this.toastr.success(this.translate.instant('LEAVE.REJECTED'));
        if (modal) modal.close();
        this.loadData();
      });
    }
  });
}

}
