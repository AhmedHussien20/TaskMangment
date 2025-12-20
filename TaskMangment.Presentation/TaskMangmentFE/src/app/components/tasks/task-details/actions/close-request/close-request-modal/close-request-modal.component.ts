import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { TaskCloseRequestAdd } from 'app/core/models/task/task-close-request';
import { TaskCloseRequestService } from 'app/core/services/task-close-request.service';

@Component({
  selector: 'app-close-request-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './close-request-modal.component.html'
})
export class CloseRequestModalComponent implements OnInit {

  @Input() taskId!: number;

  form!: FormGroup;
  isSubmitting = false;

  constructor(
    public modal: NgbActiveModal,
    private fb: FormBuilder,
    private closeRequestService: TaskCloseRequestService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      reason: ['', [Validators.required, Validators.minLength(5)]]
    });
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting) return;

    const model: TaskCloseRequestAdd = {
      message: this.form.value.reason.trim()
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
