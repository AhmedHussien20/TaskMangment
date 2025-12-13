import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';

import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { GenericTableComponent } from '../../../shared/components/generic-table/generic-table.component';

import { RoleService } from 'app/core/services/role.service';

import { TranslateModule } from '@ngx-translate/core';
import { Role } from 'app/core/models/roles/role';
import { RoleCreateUpdateComponent } from '../role-create-update/role-create-update.component';

@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule,
    NgbModalModule,
    RoleCreateUpdateComponent
  ],
  templateUrl: './role-list.component.html'
})
export class RoleListComponent implements OnInit {

  title = 'ROLE.LIST_TITLE';
  breadcrumbs = ['HOME', 'ROLES'];
  activeitem = 'ROLE.LIST_TITLE';

  columns = [
    { key: 'id', label: 'ROLE.ID' },
    { key: 'name', label: 'ROLE.NAME' },
    { key: 'description', label: 'ROLE.DESCRIPTION' }
  ];

  rows: Role[] = [];

  isLoading = false;

  constructor(
    private roleService: RoleService,
    private modalService: NgbModal
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;
    // هنا نحتاج companyId - لازم تجيبه من الـ auth أو localStorage
    const companyId = 1; // مؤقت

    this.roleService.getRoles(companyId).subscribe({
      next: (res: any) => {
        this.rows = res.data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  openAdd(modal: any) {
    this.modalService.open(modal, {
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