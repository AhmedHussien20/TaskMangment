import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subscription } from 'rxjs';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component'; 
import { TaskDetailsRefreshService } from '../../../task-details-refresh.service';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-task-penalties',
  standalone: true,
  imports: [CommonModule, GenericTableComponent, TranslateModule],
  templateUrl: './task-penalties.component.html'
})
export class TaskPenaltiesComponent implements OnInit, OnDestroy {

  @Input() taskId!: number;
  @Input() readonly = false;

  rows: any[] = [];
  totalItems = 0;

  columns = [
    { key: 'date', label: 'DATE' },
    { key: 'reason', label: 'PENALTY.REASON' },
    { key: 'amount', label: 'PENALTY.AMOUNT' },
    {
      key: 'status',
      label: 'STATUS',
      // type: 'badge',
      // badgeMap: {
      //   Active: { text: 'ACTIVE', class: 'bg-danger' },
      //   Paid: { text: 'PAID', class: 'bg-success' }
      // }
    }
  ];

  private sub!: Subscription;

  constructor(private refresh: TaskDetailsRefreshService) {}

  ngOnInit(): void {
    this.loadPenalties();
    this.sub = this.refresh.refresh$.subscribe(() => this.loadPenalties());
  }

  loadPenalties() {
    // TODO: Replace with API
    this.rows = [
      { id: 1, date: '2025-01-12', reason: 'Late delivery', amount: '500 EGP', status: 'Active' },
      { id: 2, date: '2025-01-03', reason: 'Policy breach', amount: '250 EGP', status: 'Paid' }
    ];
    this.totalItems = this.rows.length;
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
