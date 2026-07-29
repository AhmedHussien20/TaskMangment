import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';

import { Role } from 'app/core/models/roles/role';
import { RoleService } from 'app/core/services/role.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { RoleCreateUpdateComponent } from '../role-create-update/role-create-update.component';
import { RoleNotificationEditComponent } from '../role-notification-edit/role-notification-edit.component';
import { Router } from '@angular/router';
import Swal from 'sweetalert2';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    TranslateModule,
    PageHeaderComponent,
    GenericTableComponent,
    RoleCreateUpdateComponent,
    RoleNotificationEditComponent
  ],
  templateUrl: './role-list.component.html',
  styleUrls: ['./role-list.component.scss']
})
export class RoleListComponent implements OnInit {

  title = 'ROLE.LIST_TITLE';
  activeitem = 'ROLE.LIST_TITLE';
  breadcrumbs = [
    'MENU.HOME',
    'MENU.EMPLOYEES',
    'ROLE.LIST_TITLE'
  ];

  isEdit = false;
  columns = [
    { key: 'id', label: 'ROLE.ID' },
    { key: 'name', label: 'ROLE.NAME' },
    { key: 'description', label: 'ROLE.DESCRIPTION' },
    {
      key: 'employeeCount',
      label: 'ROLE.EMPLOYEES',
      type: 'icon-action' as const,
      icon: 'bi bi-people'
    },
    {
      key: 'permissionCount',
      label: 'ROLE.PERMISSIONS',
      type: 'icon-action' as const,
      icon: 'bi-shield-lock'
    },
    {
      key: 'notifyFromEmployeeCount',
      label: 'ROLE.NOTIFY_FROM_EMPLOYEES',
      type: 'icon-action' as const,
      icon: 'bi bi-bell'
    }

  ];

  rows: Role[] = [];
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
      searchKey: 'text'
    }
  };

  labels = {
    searchKey: 'ROLE.SEARCH'
  };

  isLoading = false;
  selectedRoleId: number | null = null;
  constructor(
    private roleService: RoleService,
    private modalService: NgbModal,
    private router: Router,
    private translate: TranslateService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;

    this.roleService.getAll(this.searchCriteria).subscribe({
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
    this.selectedRoleId = null;
    this.open(modal);
  }

  openEdit(id: number, modal: any) {
    this.isEdit = true;
    this.selectedRoleId = id;
    this.open(modal);
  }

  openNotifyEdit(id: number, modal: any) {
    this.selectedRoleId = id;
    this.modalService.open(modal, {
      centered: true,
      backdrop: true,
      size: 'lg',
      windowClass: 'effect-scale'
    });
  }

  open(content: any) {
    this.modalService.open(content, {
      centered: true,
      backdrop: true,
      size: 'xl',
      windowClass: 'effect-scale'
    });
  }

  onFormSubmitted() {
    this.modalService.dismissAll();
    this.loadData();
  }

  onIconAction(event: { type: string; row: any }, notifyModal: any) {
    if (event.type === 'employeeCount') {
      this.router.navigate([
        '/role',
        event.row.id,
        'employees'
      ]);
    }
    else if (event.type === 'permissionCount') {
      this.router.navigate([
        '/role',
        event.row.id,
        'permissions'
      ]);
    }
    else if (event.type === 'notifyFromEmployeeCount') {
      this.openNotifyEdit(event.row.id, notifyModal);
    }
  }
  confirmDelete(roleId: number) {
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
        this.deleteRole(roleId);
      }
    });
  }

  deleteRole(roleId: number) {
    this.isLoading = true;

    this.roleService.delete(roleId).subscribe({
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
