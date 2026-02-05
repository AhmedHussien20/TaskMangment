import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { RoleAssignmentService } from 'app/core/services/role-assignment.service';
import { RoleService } from 'app/core/services/role.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';
import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { ToastrService } from 'ngx-toastr';
import { ManagerBranchesFormComponent } from '../manager-branches-form/manager-branches-form.component';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';


@Component({
  selector: 'app-employee-role-list',
  imports: [ TranslateModule,GenericTableComponent,PageHeaderComponent,ManagerBranchesFormComponent],
  templateUrl: './employee-role-list.component.html',
  styleUrl: './employee-role-list.component.scss'
})
export class EmployeeRoleListComponent implements OnInit {

  roleId!: number;
  roleName: string = '';
  roleLevel: string | null = null;
selectedManagerId: number | null = null;
selectedBranchIds: number[] = [];

 title = '';
 breadcrumbs: string[] = [];
 activeitem = '';
  
  selectbranches: boolean = false

  columns: TableColumn[] = [
    { key: 'fullName', label: 'EMPLOYEE.NAME' },
    { key: 'email', label: 'EMPLOYEE.EMAIL' },
    { key: 'mobile', label: 'EMPLOYEE.MOBILE' },
    { key: 'branchName', label: 'EMPLOYEE.BRANCH' },
    { key: 'roleName', label: 'ROLE.ASSIGNED' }
  ];

  assignmentStatusOptions = [
  { id: null, name: 'ROLE.ALL' },
  { id: true, name: 'ROLE.ASSIGNED' },
  { id: false, name: 'ROLE.NOT_ASSIGNED' }
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
    isAssigned: null,
    filterTypes: {
      searchKey: 'text',
      isAssigned: 'dropdown' 
    }
  };
 labels = {
    searchKey: 'ROLE.SEARCH',
    isAssigned: 'ROLE.ASSIGNMENT_STATUS'

  };
  constructor(
    private route: ActivatedRoute,
    private roleAssignmentService: RoleAssignmentService,
    private roleService: RoleService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private modalService: NgbModal,
    
    

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
          id: e.employeeId,
          selected: e.isAssigned
        }));
        this.totalItems = res.data.totalCount;
      });
  }
loadRoleInfo() {
    this.roleService.getById(this.roleId).subscribe({
      next: (res) => {
        this.roleName = res.data.name;
        this.roleLevel= res.data.level
        console.log(this.roleLevel)
        if(this.roleLevel == '80')
          this.selectbranches= true

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
  this.searchCriteria = { ...this.searchCriteria, ...criteria, pageIndex: 1 };
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
openDetails(managerId: number, branchesModal: any) {
  this.selectedManagerId = managerId;
  console.log('selectedManagerId:', this.selectedManagerId);

  this.roleAssignmentService.getManagerBranches(managerId).subscribe({
    next: (res) => {
      const dto = res.data;
      this.selectedBranchIds = dto?.branchIds ?? [];

      this.modalService.open(branchesModal, {
        size: 'lg',
        backdrop: 'static',
        scrollable: true,
        centered: true
      });
    }
  });
}

onBranchesFormSubmitted(
  e: { managerId: number | null; branchIds: number[] },
  modal: any
) {
  const managerId = e.managerId ?? this.selectedManagerId;

  if (!managerId) {
    this.toastr.error(this.translate.instant('COMMON.INVALID_DATA'));
    return;
  }

  this.roleAssignmentService.setManagerBranches(managerId, e.branchIds).subscribe({
    next: () => {
      this.toastr.success(this.translate.instant('ROLE.UPDATE_SUCCESS'));
      modal.close();
    },
    error: () => {
      this.toastr.error(this.translate.instant('ROLE.UPDATE_FAILED'));
    }
  });
}


}
