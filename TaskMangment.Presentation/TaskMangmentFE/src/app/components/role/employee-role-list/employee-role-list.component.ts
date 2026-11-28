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
import Swal from 'sweetalert2';

type AssignedRoleItem = { roleId: number; roleName: string };
type AssignChoice = 'add' | 'replace' | 'cancel';

@Component({
  selector: 'app-employee-role-list',
  imports: [ TranslateModule,GenericTableComponent,PageHeaderComponent,ManagerBranchesFormComponent],
  templateUrl: './employee-role-list.component.html',
  styleUrl: './employee-role-list.component.scss'
})
export class EmployeeRoleListComponent implements OnInit {

  roleId!: number;
  roleName: string = '';
  selectedManagerId: number | null = null;
  selectedBranchIds: number[] = [];

  title = '';
  breadcrumbs: string[] = [];
  activeitem = '';

  selectbranches: boolean = false;
  requiresBranchScope = false;
  requiresEmployeeTypeScope = false;
  roleEmployeeTypeId: number | null = null;
  roleEmployeeTypeName: string | null = null;

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
    searchKey: 'ROLE.SEARCH_EMPLOYEES',
    isAssigned: 'ROLE.ASSIGNMENT_STATUS'
  };

  private readonly maxRolesPerEmployee = 2;

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
    this.loadRoleInfo();
  }

  loadData() {
    this.roleAssignmentService
      .getAssignedEmployees(this.roleId, this.searchCriteria)
      .subscribe(res => {
        this.rows = res.data.data.map((e: any) => ({
          ...e,
          id: e.employeeId,
          selected: e.isAssigned,
          assignedRoles: e.assignedRoles ?? []
        }));
        this.totalItems = res.data.totalCount;
      });
  }

  loadRoleInfo() {
    this.roleService.getById(this.roleId).subscribe({
      next: (res) => {
        this.roleName = res.data.name;
        this.requiresBranchScope = !!res.data.requiresBranchScope;
        this.requiresEmployeeTypeScope = !!res.data.requiresEmployeeTypeScope;
        this.roleEmployeeTypeId = res.data.employeeTypeId ?? null;
        this.roleEmployeeTypeName = res.data.employeeTypeName ?? null;
        // Type coverage is fixed on the role and applied on assign — no per-employee type picker.
        this.selectbranches = false;

        this.translate.get('ROLE.ASSIGNEMPLOYEE').subscribe(assignText => {
          this.title = `${assignText}  ${this.roleName}`;
        });

        this.translate.get('ROLE.ASSIGN_TO_EMPLOYEE').subscribe(assignToText => {
          this.activeitem = `${assignToText}  ${this.roleName}`;
        });
        this.breadcrumbs = ['MENU.HOME', 'MENU.EMPLOYEES', 'ROLE.LIST_TITLE', this.roleName, 'ROLE.ASSIGNEMPLOYEE'];

        this.loadData();
      },
      error: () => {
        this.title = 'ROLE.ASSIGN_TO_EMPLOYEE';
        this.breadcrumbs = ['MENU.HOME', 'MENU.EMPLOYEES', 'ROLE.LIST_TITLE', 'ROLE.ASSIGNEMPLOYEE'];
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

  private otherRoles(emp: any): AssignedRoleItem[] {
    const roles: AssignedRoleItem[] = emp?.assignedRoles ?? [];
    return roles.filter(r => r.roleId !== this.roleId);
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  private boldRolesHtml(roles: AssignedRoleItem[] | string): string {
    const names = typeof roles === 'string'
      ? roles.split(',').map(x => x.trim()).filter(Boolean)
      : roles.map(r => r.roleName.trim()).filter(Boolean);

    return names
      .map(name => `<strong>${this.escapeHtml(name)}</strong>`)
      .join(', ');
  }

  private async askAddOrReplace(otherRoles: AssignedRoleItem[]): Promise<AssignChoice> {
    const result = await Swal.fire({
      title: this.translate.instant('ROLE.CONFIRM_ADD_OR_REPLACE_TITLE'),
      html: this.translate.instant('ROLE.CONFIRM_ADD_OR_REPLACE_TEXT', {
        roles: this.boldRolesHtml(otherRoles),
        newRole: this.escapeHtml(this.roleName)
      }),
      icon: 'question',
      showDenyButton: true,
      showCancelButton: true,
      confirmButtonText: this.translate.instant('ROLE.CONFIRM_ADD_ROLE'),
      denyButtonText: this.translate.instant('ROLE.CONFIRM_REPLACE_ROLE'),
      cancelButtonText: this.translate.instant('COMMON.CANCEL_BUTTON'),
      confirmButtonColor: '#6f42c1',
      denyButtonColor: '#0d6efd',
      cancelButtonColor: '#6c757d'
    });

    if (result.isConfirmed) return 'add';
    if (result.isDenied) return 'replace';
    return 'cancel';
  }

  private async showMaxRolesPopup(otherRoles: AssignedRoleItem[]): Promise<void> {
    await Swal.fire({
      title: this.translate.instant('ROLE.MAX_ROLES_POPUP_TITLE'),
      html: this.translate.instant('ROLE.MAX_ROLES_POPUP_TEXT', {
        roles: this.boldRolesHtml(otherRoles),
        newRole: this.escapeHtml(this.roleName)
      }),
      icon: 'info',
      confirmButtonText: this.translate.instant('COMMON.OK'),
      confirmButtonColor: '#6f42c1'
    });
  }

  private applyAssignment(
    emp: any,
    assign: boolean,
    unassignRoleIds?: number[]
  ) {
    emp.isAssigned = assign;
    emp.selected = assign;

    this.roleAssignmentService
      .bulkAssignEmployees(this.roleId, [{
        employeeId: emp.employeeId,
        assign,
        unassignRoleIds: unassignRoleIds?.length ? unassignRoleIds : undefined
      }])
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('ROLE.ASSIGNED_TO_EMPLOYEE_SUCCESS'));
          this.loadData();
        },
        error: async (err) => {
          emp.isAssigned = !assign;
          emp.selected = !assign;
          if (err?.error?.errorCode === 'EMPLOYEE_MAX_ROLES_EXCEEDED') {
            await this.showMaxRolesPopup(this.otherRoles(emp));
          }
        }
      });
  }

  async saveAssignments() {
    const newlyAssigning = this.rows.filter(r =>
      r.selected === true &&
      r.isAssigned !== true
    );

    for (const emp of newlyAssigning) {
      const others = this.otherRoles(emp);
      if (others.length >= this.maxRolesPerEmployee) {
        await this.showMaxRolesPopup(others);
        return;
      }
      if (others.length === 1) {
        const choice = await this.askAddOrReplace(others);
        if (choice === 'cancel') return;
        if (choice === 'replace') {
          this.applyAssignment(emp, true, others.map(r => r.roleId));
          return;
        }
      }
    }

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
        error: async (err) => {
          if (err?.error?.errorCode === 'EMPLOYEE_MAX_ROLES_EXCEEDED') {
            const sample = newlyAssigning[0];
            await this.showMaxRolesPopup(sample ? this.otherRoles(sample) : []);
          }
        }
      });
  }

  async onAssignToggle(event: { row: any; checked: boolean }) {
    const emp = event.row;
    const checked = event.checked;
    const wasAssignedToThis = emp.isAssigned === true;

    if (checked && !wasAssignedToThis) {
      const others = this.otherRoles(emp);

      if (others.length >= this.maxRolesPerEmployee) {
        emp.selected = false;
        emp.isAssigned = false;
        await this.showMaxRolesPopup(others);
        return;
      }

      if (others.length === 1) {
        const choice = await this.askAddOrReplace(others);
        if (choice === 'cancel') {
          emp.selected = false;
          emp.isAssigned = false;
          return;
        }
        if (choice === 'replace') {
          this.applyAssignment(emp, true, others.map(r => r.roleId));
          return;
        }
      }
    }

    this.applyAssignment(emp, checked);
  }

  openDetails(managerId: number, branchesModal: any) {
    this.selectedManagerId = managerId;

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
    e: { managerId: number | null; employeeTypeId?: number; branchIds: number[] },
    modal: any
  ) {
    const managerId = e.managerId ?? this.selectedManagerId;

    if (!managerId) {
      this.toastr.error(this.translate.instant('COMMON.INVALID_DATA'));
      return;
    }

    this.roleAssignmentService.setManagerBranches(managerId, {
      employeeTypeId: e.employeeTypeId,
      branchIds: e.branchIds
    })
      .subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('ROLE.UPDATE_SUCCESS'));
          modal.close();
        },
        error: () => {
          this.toastr.error(this.translate.instant('ROLE.UPDATE_FAILED'));
        }
      });
  }

  disableShowDetailsRow = (row: any) => {
    return row.isAssigned !== true;
  };

}
