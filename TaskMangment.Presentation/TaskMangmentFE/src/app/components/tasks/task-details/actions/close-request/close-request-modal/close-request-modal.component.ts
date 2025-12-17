import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { TaskCloseRequestAdd } from 'app/core/models/task/task-close-request';
import { TaskCloseRequestService } from 'app/core/services/task-close-request.service';

@Component({
  selector: 'app-close-request-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './close-request-modal.component.html'
})
export class CloseRequestModalComponent {

  @Input() taskId!: number;

  reason = '';
  isSubmitting = false;

  constructor(
    public modal: NgbActiveModal,
    private closeRequestService: TaskCloseRequestService
  ) {}

  submit(): void {
    if (!this.reason.trim() || this.isSubmitting) return;

    const model: TaskCloseRequestAdd = {
      message: this.reason.trim()
    };

    this.isSubmitting = true;

    this.closeRequestService.create(this.taskId, model).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.modal.close(true); // notify parent to refresh
      },
      error: (err) => {
        console.error('Failed to submit close request', err);
        this.isSubmitting = false;
      }
    });
  }
}
