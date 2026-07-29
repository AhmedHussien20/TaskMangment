import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  NgbModal,
  NgbModalModule,
  NgbPaginationModule,
} from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { LeaveService } from 'app/core/services/leave.service';
import { LeaveGetDto } from 'app/core/models/leave/leave';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import {
  GenericTableComponent,
  TableColumn,
} from 'app/shared/components/generic-table/generic-table.component';
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
    LeaveCreateUpdateComponent,
  ],
  templateUrl: './leave-list.component.html',
  styleUrls: ['./leave-list.component.scss'],
})
export class LeaveListComponent implements OnInit {
  canCreate = false;
  canApprove = false;
  canReject = false;
  status: { id: number; name: string }[] = [];

  /** 'all' = LeaveRequests; 'pending' = pending for approval */
  listMode: 'all' | 'pending' = 'all';

  title = 'LEAVE.LIST_TITLE';
  activeitem = 'LEAVE.LIST_TITLE';
  breadcrumbs = ['MENU.HOME', 'MENU.LEAVES', 'LEAVE.LIST_TITLE'];

  columns: TableColumn[] = [
    { key: 'id', label: 'LEAVE.ID' },
    { key: 'employeeName', label: 'LEAVE.EMPLOYEE' },
    { key: 'leaveTypeName', label: 'LEAVE.TYPE' },
    { key: 'startDate', label: 'LEAVE.START_DATE', type: 'date' },
    { key: 'endDate', label: 'LEAVE.END_DATE', type: 'date' },
    {
      key: 'statusName',
      label: 'LEAVE.STATUS',
      type: 'badge',
      badgeMap: {
        Pending: {
          text: 'LEAVE.PENDING',
          class: 'bg-warning',
        },
        Approved: {
          text: 'LEAVE.APPROVED',
          class: 'bg-success',
        },
        Rejected: {
          text: 'LEAVE.REJECTED',
          class: 'bg-danger',
        },
      },
    },
  ];

  reviewModel = {
    status: null as 'Approved' | 'Rejected' | null,
    rejectReason: '',
  };

  rows: LeaveGetDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  statusOptions = [
    { id: 1, name: 'LEAVE.PENDING' },
    { id: 2, name: 'LEAVE.APPROVED' },
    { id: 3, name: 'LEAVE.REJECTED' },
  ];

  searchCriteria: SearchCriteria = {
    searchKey: '',
    statusId: null,
    pageIndex: this.page,
    pageSize: this.entries,
    sortColumn: 'Id',
    sortDirection: 'DESC',
    filterTypes: {
      searchKey: 'text',
      statusId: 'dropdown',
    },
  };

  labels = {
    searchKey: 'LEAVE.SEARCH',
    statusId: 'LEAVE.STATUS',
  };
  isLoading = false;

  isEdit = false;
  selectedLeaveId: number | null = null;
  selectedLeave: LeaveGetDto | null = null;
  @ViewChild('detailsModal') detailsModal: any;

  constructor(
    private leaveService: LeaveService,
    private modalService: NgbModal,
    private toastr: ToastrService,
    private translate: TranslateService,
    private auth: AuthService
  ) {}

  ngOnInit(): void {
    const roleLevel = this.auth.getRoleLevel();

    this.status = this.statusOptions;
    this.canCreate = roleLevel >= 10;
    this.canApprove = this.auth.hasPermission('APPROVE_LEAVE');
    this.canReject = this.auth.hasPermission('REJECT_LEAVE');

    this.loadData();
  }

  get canReview(): boolean {
    return this.canApprove || this.canReject;
  }

  setListMode(mode: 'all' | 'pending'): void {
    if (this.listMode === mode) return;
    this.listMode = mode;
    this.page = 1;
    this.searchCriteria.pageIndex = 1;
    if (mode === 'pending') {
      this.searchCriteria['statusId'] = null;
    }
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    const request$ =
      this.listMode === 'pending' && this.canReview
        ? this.leaveService.getPending(this.searchCriteria)
        : this.leaveService.LeaveRequests(this.searchCriteria);

    request$.subscribe({
      next: (res: any) => {
        const pageData = res.data;

        this.rows = pageData.data ?? [];
        this.totalItems = pageData.totalCount ?? 0;
        this.page = pageData.pageIndex ?? 1;
        this.entries = pageData.pageSize ?? 10;

        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      },
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
      pageIndex: 1,
    };

    this.page = 1;
    this.loadData();
  }

  openAdd(modal: any) {
    this.isEdit = false;
    this.selectedLeaveId = null;
    this.modalService.open(modal, { size: 'lg', centered: true });
  }

  openEdit(id: number, modal: any) {
    this.isEdit = true;
    this.selectedLeaveId = id;
    this.modalService.open(modal, { size: 'lg', centered: true });
  }

  onFormSubmitted() {
    this.modalService.dismissAll();
    this.loadData();
  }

  openDetailsModal(id: number) {
    this.reviewModel = {
      status: null,
      rejectReason: '',
    };

    this.leaveService.getById(id).subscribe({
      next: (res: any) => {
        this.selectedLeave = res.data;
        this.isLoading = false;
        this.modalService.open(this.detailsModal, {
          size: 'lg',
          centered: true,
        });
      },
      error: () => (this.isLoading = false),
    });
  }

  confirmReview(modal: any) {
    if (!this.selectedLeave || !this.reviewModel.status) return;

    if (this.reviewModel.status === 'Approved') {
      if (!this.canApprove) {
        this.toastr.error(this.translate.instant('FORBIDDEN.MESSAGE'));
        return;
      }
      this.leaveService.approve(this.selectedLeave.id).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('LEAVE.APPROVED_SUCCESS'));
          modal.close();
          this.loadData();
        },
        error: () => {
          this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
        },
      });
    }

    if (this.reviewModel.status === 'Rejected') {
      if (!this.canReject) {
        this.toastr.error(this.translate.instant('FORBIDDEN.MESSAGE'));
        return;
      }
      if (!this.reviewModel.rejectReason) {
        this.toastr.error(
          this.translate.instant('LEAVE.REJECT_REASON_REQUIRED')
        );
        return;
      }

      this.leaveService
        .reject(this.selectedLeave.id, this.reviewModel.rejectReason)
        .subscribe({
          next: () => {
            this.toastr.success(this.translate.instant('LEAVE.REJECTED_SUCCESS'));
            modal.close();
            this.loadData();
          },
          error: () => {
            this.toastr.error(this.translate.instant('COMMON.ERROR_LOADING_DATA'));
          },
        });
    }
  }
}
