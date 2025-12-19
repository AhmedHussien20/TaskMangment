import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { TaskExtensionRequestService } from 'app/core/services/task-extension-request.service';
import { TaskExtensionRequestAdd } from 'app/core/models/task/task-extension-request';

@Component({
  selector: 'app-extend-request-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './extend-request.component.html'
})
export class ExtendRequestComponent {

  @Input() taskId!: number;

  form!: FormGroup;
  isSubmitting = false;

  constructor(
    public modal: NgbActiveModal,
    private fb: FormBuilder,
    private extendRequestService: TaskExtensionRequestService
  ) {}

  ngOnInit() {
    this.form = this.fb.group({
      reason: ['', [Validators.required, Validators.minLength(5)]]
    });
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting) return;

    const model: TaskExtensionRequestAdd = {
      reason: this.form.value.reason.trim()
    };

    this.isSubmitting = true;

    this.extendRequestService.create(this.taskId, model).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.modal.close(true);
      },
      error: (err) => {
        console.error('Failed to submit extend request', err);
        this.isSubmitting = false;
      }
    });
  }
}
