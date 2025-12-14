import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule } from '@ng-bootstrap/ng-bootstrap';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { RoleService } from 'app/core/services/role.service';
import { TranslateModule } from '@ngx-translate/core';
import { Role } from 'app/core/models/roles/role';
import { RoleCreateUpdateComponent } from "../role-create-update/role-create-update.component";
import { RoleAssignEmployeeComponent } from '../role-assign-employee/role-assign-employee.component';
import { RoleAssignPermissionsComponent } from '../role-assign-permission/role-assign-permission.component';


@Component({
  selector: 'app-role-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    PageHeaderComponent,
    TranslateModule,
    NgbModalModule,
    RoleCreateUpdateComponent,
    RoleAssignEmployeeComponent,
    RoleAssignPermissionsComponent

],
  templateUrl: './role-list.component.html'
})
export class RoleListComponent implements OnInit {

  title = 'ROLE.LIST_TITLE';
  breadcrumbs = ['HOME', 'ROLES'];
  activeitem = 'ROLE.LIST_TITLE';

  roles: Role[] = [];
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

    this.roleService.getRoles().subscribe({
      next: (res: any) => {
        this.roles = res.data;
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
   openAssignPermissions(modal: any) {
    this.modalService.open(modal, {
      centered: true,
      backdrop: true,
      size: 'lg',
      windowClass: 'effect-scale'
    });
  }

  openAssignToEmployee(modal: any) {
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