import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-penalty-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  template: `
    <div class="modal-header">
      <h6>{{ 'PENALTY.ADD' | translate }}</h6>
      <button class="btn-close" (click)="modal.dismiss()"></button>
    </div>

    <div class="modal-body">
      <input class="form-control mb-2" type="number"
        [(ngModel)]="amount"
        placeholder="{{ 'PENALTY.AMOUNT' | translate }}" />

      <textarea class="form-control" rows="3"
        [(ngModel)]="reason"
        placeholder="{{ 'PENALTY.REASON' | translate }}">
      </textarea>
    </div>

    <div class="modal-footer">
      <button class="btn btn-secondary" (click)="modal.dismiss()">
        {{ 'FORM.CANCEL' | translate }}
      </button>
      <button class="btn btn-danger" (click)="submit()">
        {{ 'FORM.SAVE' | translate }}
      </button>
    </div>
  `
})
export class PenaltyModalComponent {
  @Input() taskId!: number;
  amount!: number;
  reason = '';

  constructor(public modal: NgbActiveModal) {}

  submit() {
    if (!this.amount || !this.reason.trim()) return;
    // TODO: API
    this.modal.close(true);
  }
}
