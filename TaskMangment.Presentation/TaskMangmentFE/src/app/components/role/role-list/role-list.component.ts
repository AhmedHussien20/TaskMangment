import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';

import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';

import { Role } from 'app/core/models/roles/role';
import { RoleService } from 'app/core/services/role.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { RoleCreateUpdateComponent } from '../role-create-update/role-create-update.component';
import { Router } from '@angular/router';

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
    RoleCreateUpdateComponent
  ],
  templateUrl: './role-list.component.html',
  styleUrls: ['./role-list.component.scss']
})
export class RoleListComponent implements OnInit {

  title = 'ROLE.LIST_TITLE';
  breadcrumbs = ['HOME', 'ROLES'];
  activeitem = 'ROLE.LIST_TITLE';
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
    sortDirection: 'ASC',
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
    private router: Router
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

  onIconAction(event: { type: string; row: any }) {
    if (event.type === 'employeeCount') {
      this.router.navigate([
        '/role',
        event.row.id,
        'employees'
      ]);
    }
    else if (event.type === 'permissionCount') 
    {
      this.router.navigate([
        '/role',
        event.row.id,
        'permissions'
      ]);
    }

  }
  
}
