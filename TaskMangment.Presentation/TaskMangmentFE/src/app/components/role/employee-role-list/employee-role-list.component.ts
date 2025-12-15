import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { RoleAssignmentService } from 'app/core/services/role-assignment.service';
import { SearchCriteria } from 'app/models/search-criteria.model';
import { GenericTableComponent, TableColumn } from 'app/shared/components/generic-table/generic-table.component';

@Component({
  selector: 'app-employee-role-list',
  imports: [ TranslateModule,GenericTableComponent],
  templateUrl: './employee-role-list.component.html',
  styleUrl: './employee-role-list.component.scss'
})
export class EmployeeRoleListComponent implements OnInit {

  roleId!: number;

  title = 'ROLE.ASSIGN_EMPLOYEES';

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
    sortDirection: 'ASC',
    filterTypes: {
      searchKey: 'text'
    }
  };
 labels = {
    searchKey: 'ROLE.SEARCH'
  };
  constructor(
    private route: ActivatedRoute,
    private roleAssignmentService: RoleAssignmentService
  ) {}

  ngOnInit() {
    this.roleId = Number(this.route.snapshot.paramMap.get('roleId'));
    this.loadData();
  }

  loadData() {
    this.roleAssignmentService
      .getAssignedEmployees(this.roleId, this.searchCriteria)
      .subscribe(res => {
        this.rows = res.data.data.map((e: any) => ({
          ...e,
          selected: e.isAssigned // ⭐ مهم
        }));
        this.totalItems = res.data.totalCount;
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
      .subscribe(() => {
        this.loadData();
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
      error: () => {
        emp.isAssigned = !checked;
      }
    });
}

}
