import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { Permissions } from 'app/core/constants/permissions';
import { AuthService } from 'app/core/services/auth.service';
import { ReportListService } from 'app/core/services/report-list.service';

interface ReportRoleOption {
  id: number;
  name: string;
  level: number;
}

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
  roles: ReportRoleOption[] = [];

  constructor(
    private reportListService: ReportListService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.canFilterByRole =
      this.authService.hasAnyPermission(
        Permissions.VIEW_SCOPED_REPORTS,
        Permissions.VIEW_COMPANY_REPORTS,
        Permissions.VIEW_SCOPED_TASKS,
        Permissions.VIEW_COMPANY_TASKS,
        Permissions.VIEW_EMPLOYEES
      ) || this.authService.hasAccessScope();

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
    this.reportListService.getFilterRoles().subscribe({
      next: (res) => {
        this.roles = res.data ?? [];
      }
    });
  }
}
