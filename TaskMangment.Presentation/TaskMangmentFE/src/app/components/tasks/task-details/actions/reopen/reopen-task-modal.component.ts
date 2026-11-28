import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { TaskService } from 'app/core/services/task.service';
import {
  notPastDueDateValidator,
  weekendDueDateValidator
} from 'app/shared/validations/weekend-due-date.validator';

@Component({
  selector: 'app-reopen-task-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './reopen-task-modal.component.html'
})
export class ReopenTaskModalComponent {
  @Input() taskId!: number;
  @Input() dueDateRequired = false;

  form!: FormGroup;
  isSubmitting = false;

  constructor(
    public modal: NgbActiveModal,
    private fb: FormBuilder,
    private taskService: TaskService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    const dueDateValidators = this.dueDateRequired
      ? [Validators.required, weekendDueDateValidator(), notPastDueDateValidator()]
      : [weekendDueDateValidator(), notPastDueDateValidator()];

    this.form = this.fb.group({
      dueDate: ['', dueDateValidators]
    });
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting) return;

    const dueDate = this.form.value.dueDate ? String(this.form.value.dueDate) : undefined;
    this.isSubmitting = true;

    this.taskService.reopen(this.taskId, dueDate).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('TASK.REOPEN_SUCCESS'));
        this.isSubmitting = false;
        this.modal.close(true);
      },
      error: () => {
        this.isSubmitting = false;
      }
    });
  }
}
