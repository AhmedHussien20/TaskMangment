import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Role } from 'app/core/models/roles/role';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { AuthService } from 'app/core/services/auth.service';
import { RoleService } from 'app/core/services/role.service';

@Component({
  selector: 'app-report-role-filter',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  template: `
    <div *ngIf="canFilterByRole" class="d-flex flex-column">
      <label class="form-label small">{{ 'REPORTS.ROLE_TITLE' | translate }}</label>
      <select class="form-select form-select-sm" [(ngModel)]="selectedRoleId" (ngModelChange)="onRoleChange($event)">
        <option [ngValue]="undefined">{{ 'REPORTS.ALL_ROLES' | translate }}</option>
        <option *ngFor="let role of roles" [ngValue]="role.id">{{ role.name }}</option>
      </select>
    </div>
  `
})
export class ReportRoleFilterComponent implements OnInit {
  @Input() selectedRoleId?: number;
  @Output() selectedRoleIdChange = new EventEmitter<number | undefined>();

  canFilterByRole = false;
  roles: Role[] = [];

  constructor(
    private roleService: RoleService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.canFilterByRole = (this.authService.getRoleLevel() ?? 0) >= 100;
    if (this.canFilterByRole) {
      this.loadRoles();
    }
  }

  get selectedRoleTitle(): string | undefined {
    return this.roles.find(r => r.id === Number(this.selectedRoleId))?.name;
  }

  onRoleChange(value: number | undefined): void {
    this.selectedRoleIdChange.emit(value);
  }

  private loadRoles(): void {
    const criteria: SearchCriteria = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 500,
      sortColumn: 'Name',
      sortDirection: 'ASC'
    };

    this.roleService.getAll(criteria).subscribe({
      next: (res: any) => {
        this.roles = res.data?.data ?? [];
      }
    });
  }
}
