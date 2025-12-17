import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { TaskExtensionRequestService } from 'app/core/services/task-extension-request.service';
import { TaskExtensionRequestAdd } from 'app/core/models/task/task-extension-request';

@Component({
  selector: 'app-extend-request-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './extend-request.component.html'
})
export class ExtendRequestComponent {

  @Input() taskId!: number;

  reason = '';
  isSubmitting = false;

  constructor(
    public modal: NgbActiveModal,
    private extendRequestService: TaskExtensionRequestService
  ) {}

  submit(): void {
    if (!this.reason.trim() || this.isSubmitting) return;

    const model: TaskExtensionRequestAdd = {
      reason: this.reason.trim()
    };

    this.isSubmitting = true;

    this.extendRequestService.create(this.taskId, model).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.modal.close(true); // نفس Close Request
      },
      error: (err) => {
        console.error('Failed to submit extend request', err);
        this.isSubmitting = false;
      }
    });
  }
}
