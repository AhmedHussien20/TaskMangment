import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-close-request-modal',
  standalone: true,
  imports: [CommonModule, FormsModule,TranslateModule],
  templateUrl: './close-request-modal.component.html'
})
export class CloseRequestModalComponent {

  @Input() taskId!: number;

  reason = '';
  isSubmitting = false;

  constructor(public modal: NgbActiveModal) {}

  submit(): void {
    if (!this.reason.trim()) return;

    this.isSubmitting = true;

    console.log('Close request submitted', {
      taskId: this.taskId,
      reason: this.reason
    });

    this.modal.close(true);
  }
}
