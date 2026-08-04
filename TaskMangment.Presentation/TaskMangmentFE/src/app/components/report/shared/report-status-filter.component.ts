import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

export interface ReportStatusOption {
  label: string;
  value: string | undefined;
}

export const REPORT_STATUS_OPTIONS: ReportStatusOption[] = [
  { label: 'REPORTS.ALL_STATUSES', value: undefined },
  { label: 'TASK.STATUS_NEW', value: 'New' },
  { label: 'TASK.STATUS_IN_PROGRESS', value: 'InProgress' },
  { label: 'TASK.STATUS_CLOSED', value: 'Closed' },
  { label: 'TASK.STATUS_ARCHIVED', value: 'Archived' },
  { label: 'TASK.STATUS_AUTOCLOSE', value: 'AutoClose' },
];

@Component({
  selector: 'app-report-status-filter',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  template: `
    <div class="d-flex flex-column">
      <label class="form-label small">{{ 'REPORTS.STATUS' | translate }}</label>
      <select
        class="form-select form-select-sm"
        [ngModel]="status"
        (ngModelChange)="onStatusChange($event)">
        <option *ngFor="let s of options" [ngValue]="s.value">{{ s.label | translate }}</option>
      </select>
    </div>
  `
})
export class ReportStatusFilterComponent {
  @Input() status?: string;
  @Input() options: ReportStatusOption[] = REPORT_STATUS_OPTIONS;
  @Output() statusChange = new EventEmitter<string | undefined>();

  onStatusChange(value: string | undefined): void {
    this.statusChange.emit(value || undefined);
  }
}
