import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RoleAssignmentService } from 'app/core/services/role-assignment.service';
import { RoleService } from 'app/core/services/role.service';
import { SearchCriteria } from 'app/models/search-criteria.model';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { ToastrService } from 'ngx-toastr';


@Component({
  selector: 'app-employee-role-list',
  imports: [ TranslateModule,GenericTableComponent,PageHeaderComponent],
  templateUrl: './employee-role-list.component.html',
  styleUrl: './employee-role-list.component.scss'
})
export class EmployeeRoleListComponent implements OnInit {

  roleId!: number;
  roleName: string = '';

 title = '';
  breadcrumbs: string[] = [];
  activeitem = '';


  columns: TableColumn[] = [
    { key: 'fullName', label: 'EMPLOYEE.NAME' },
    { key: 'email', label: 'EMPLOYEE.EMAIL' },
    { key: 'mobile', label: 'EMPLOYEE.MOBILE' },
    { key: 'branchName', label: 'EMPLOYEE.BRANCH' }
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
    sortDirection: 'DESC',
    filterTypes: {
      searchKey: 'text'
    }
  };
 labels = {
    searchKey: 'ROLE.SEARCH'
  };
  constructor(
    private route: ActivatedRoute,
    private roleAssignmentService: RoleAssignmentService,
    private roleService: RoleService,
    private toastr: ToastrService,
    private translate: TranslateService

  ) {}

  ngOnInit() {
    this.roleId = Number(this.route.snapshot.paramMap.get('roleId'));
    //this.loadData();
    this.loadRoleInfo();

  }

  loadData() {
    this.roleAssignmentService
      .getAssignedEmployees(this.roleId, this.searchCriteria)
      .subscribe(res => {
        this.rows = res.data.data.map((e: any) => ({
          ...e,
          selected: e.isAssigned
        }));
        this.totalItems = res.data.totalCount;
      });
  }
loadRoleInfo() {
    this.roleService.getById(this.roleId).subscribe({
      next: (res) => {
        this.roleName = res.data.name;
        
        this.translate.get('ROLE.ASSIGNEMPLOYEE').subscribe(assignText => {
      this.title = `${assignText}  ${this.roleName}`;
    });
    
    this.translate.get('ROLE.ASSIGN_TO_EMPLOYEE').subscribe(assignToText => {
      this.activeitem = `${assignToText}  ${this.roleName}`;
    });
        this.breadcrumbs = ['MENU.HOME','MENU.EMPLOYEES','ROLE.LIST_TITLE', this.roleName, 'ROLE.ASSIGNEMPLOYEE'];
 
        this.loadData();
      },
      error: () => {
        this.title = 'ROLE.ASSIGN_TO_EMPLOYEE';
        this.breadcrumbs = ['MENU.HOME','MENU.EMPLOYEES','ROLE.LIST_TITLE','ROLE.ASSIGNEMPLOYEE'];
        this.activeitem = 'ROLE.ASSIGN_TO_EMPLOYEE';
        this.loadData();
      }
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
      employeeId: r.employeeId,
      assign: r.selected === true
    }));

    this.roleAssignmentService
      .bulkAssignEmployees(this.roleId, assignments)
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('ROLE.ASSIGNED_TO_EMPLOYEE_SUCCESS'));
          this.loadData();
        },
       
      });
  }

  onAssignToggle(event: { row: any; checked: boolean }) {
  const emp = event.row;
  const checked = event.checked;

  emp.isAssigned = checked;

  const payload = {
    assignments: [
      {
        employeeId: emp.employeeId,
        assign: checked
      }
    ]
  };

  this.roleAssignmentService
    .bulkAssignEmployees(this.roleId, payload.assignments)
     .subscribe({
           next: () => {
          this.toastr.success(this.translate.instant('ROLE.ASSIGNED_TO_EMPLOYEE_SUCCESS'));
        },
        error: () => {
          emp.isAssigned = !checked;

        }
    });
}

}
