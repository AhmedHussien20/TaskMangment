import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateModule,TranslateService  } from '@ngx-translate/core';
import { RolePermissionService } from 'app/core/services/role-permission.service';
import { RoleService } from 'app/core/services/role.service'; // Service جديد
import { SearchCriteria } from 'app/models/search-criteria.model';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-role-permission-list',
  imports: [TranslateModule, GenericTableComponent, PageHeaderComponent],
  templateUrl: './role-permission-list.component.html',
  styleUrl: './role-permission-list.component.scss'
})
export class RolePermissionListComponent implements OnInit {
  roleId!: number;
  roleName: string = '';
  
  title = '';
  breadcrumbs: string[] = [];
  activeitem = '';

  columns: TableColumn[] = [
    { key: 'code', label: 'PERMISSION.CODE' },
    { key: 'name', label: 'PERMISSION.NAME' },
    { key: 'description', label: 'PERMISSION.DESCRIPTION' }
  ];

  rows: any[] = [];
  totalItems = 0;
  page = 1;
  entries = 20;

  searchCriteria: SearchCriteria = {
    searchKey: '',
    pageIndex: 1,
    pageSize: 20,
    sortColumn: '',
    sortDirection: 'ASC',
    filterTypes: {
      searchKey: 'text'
    }
  };

  labels = {
    searchKey: 'ROLE.SEARCH'
  };

  isLoading = true;

  constructor(
    private route: ActivatedRoute,
    private rolePermissionService: RolePermissionService,
    private roleService: RoleService,
    private toastr: ToastrService,
    private translate: TranslateService 

  ) {}

  ngOnInit() {
    this.roleId = Number(this.route.snapshot.paramMap.get('roleId'));
    this.loadRoleInfo();
  }

  loadRoleInfo() {
    this.roleService.getById(this.roleId).subscribe({
      next: (res) => {
        this.roleName = res.data.name;
        
         this.translate.get('ROLE.PERMISSIONROLE').subscribe(assignText => {
      this.title = `${assignText}  ${this.roleName}`;
    });
    
    this.translate.get('ROLE.PERMISSIONROLE').subscribe(assignToText => {
      this.activeitem = `${assignToText}  ${this.roleName}`;
    });
        this.breadcrumbs = ['MENU.HOME','MENU.EMPLOYEES','ROLE.LIST_TITLE', this.roleName, 'ROLE.PERMISSIONS'];
 
        
        this.loadData();
      },
      error: () => {
        this.title = 'ROLE.ASSIGN_PERMISSIONS';
        this.breadcrumbs = ['HOME', 'ROLES', 'PERMISSIONS'];
        this.activeitem = 'ROLE.ASSIGN_PERMISSIONS';
        this.loadData();
      }
    });
  }

  loadData() {
    this.isLoading = true;
    this.rolePermissionService
      .getAssignedPermissions(this.roleId, this.searchCriteria)
      .subscribe(res => {
        this.rows = res.data.data.map((p: any) => ({
          ...p,
          selected: p.isAssigned
        }));
        this.totalItems = res.data.totalCount;
        this.isLoading = false;
      });
  }

  onPageChange(page: number) {
    this.searchCriteria.pageIndex = page;
    this.loadData();
  }

  onEntriesChange(entries: number) {
    this.searchCriteria.pageSize = entries;
    this.searchCriteria.pageIndex = 1;
    this.loadData();
  }

  applyFilters = (criteria: any) => {
    this.searchCriteria = { ...criteria, pageIndex: 1 };
    this.loadData();
  };

  saveAssignments() {
    const assignments = this.rows.map(r => ({
      permissionId: r.permissionId,
      assign: r.selected === true
    }));

   this.rolePermissionService
      .bulkAssignPermissions(this.roleId, assignments)
      .subscribe({
        next: () => {
          this.toastr.success('ROLE.PERMISSIONS_ASSIGNED_SUCCESS', 'Success');
          this.loadData();
        },
        error: (error) => {
          console.error('Error saving assignments:', error);
          this.toastr.error('ROLE.PERMISSIONS_ASSIGNED_FAILED', 'Error');
        }
      });
  }

  onAssignToggle(event: { row: any; checked: boolean }) {
    const perm = event.row;
    const checked = event.checked;

    perm.isAssigned = checked;

    const payload = {
      assignments: [
        {
          permissionId: perm.permissionId,
          assign: checked
        }
      ]
    };

    this.rolePermissionService
      .bulkAssignPermissions(this.roleId, payload.assignments)
      .subscribe({
           next: () => {
          this.toastr.success('ROLE.PERMISSIONS_ASSIGNED_SUCCESS', 'Success');
        },
        error: () => {
          perm.isAssigned = !checked;
          this.toastr.error('ROLE.PERMISSIONS_ASSIGNED_FAILED', 'Error');

        }
      });
  }
}