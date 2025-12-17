import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component'; 
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-task-warnings',
  standalone: true,
  imports: [CommonModule, GenericTableComponent, TranslateModule],
  templateUrl: './task-warnings.component.html'
})
export class TaskWarningsComponent implements OnInit, OnDestroy {

  @Input() taskId!: number;
  @Input() readonly = false;

  rows: any[] = [];
  totalItems = 0;

  columns = [
    { key: 'date', label: 'DATE' },
    { key: 'reason', label: 'WARNING.REASON' },
    {
      key: 'status',
      label: 'STATUS',
      //type: 'badge'
      //,
      // badgeMap: {
      //   Active: { text: 'ACTIVE', class: 'bg-warning' },
      //   Closed: { text: 'CLOSED', class: 'bg-secondary' }
      // }
    }
  ];

  private sub!: Subscription;

  constructor(private refresh: TaskDetailsRefreshService) {}

  ngOnInit(): void {
    this.loadWarnings();

    this.sub = this.refresh.refresh$
      .subscribe(() => this.loadWarnings());
  }

  loadWarnings() {
    this.rows = [
      { id: 1, date: '2025-01-10', reason: 'Late submission', status: 'Active' },
      { id: 2, date: '2025-01-05', reason: 'Missing docs', status: 'Closed' }
    ];
    this.totalItems = this.rows.length;
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
