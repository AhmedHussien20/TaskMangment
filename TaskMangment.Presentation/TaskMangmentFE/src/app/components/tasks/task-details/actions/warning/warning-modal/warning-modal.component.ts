import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { TaskService } from 'app/core/services/task.service';
import { TaskWarningService } from 'app/core/services/task-warning.service';
import { WarningAddEditDto } from 'app/core/models/task/task-warning';
import { SimpleEmployee } from 'app/core/models/task/task';

@Component({
  selector: 'app-warning-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './warning-modal.component.html'
})
export class WarningModalComponent implements OnInit {

  @Input() taskId!: number;

  form!: FormGroup;
  employees: SimpleEmployee[] = [];
  isSubmitting = false;

  constructor(
    public modal: NgbActiveModal,
    private fb: FormBuilder,
    private warningService: TaskWarningService,
    private taskService: TaskService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      selectedEmployeeId: [null, Validators.required],
      reason: ['', [Validators.required, Validators.minLength(5)]]
    });

    this.loadEmployees();
  }

  loadEmployees(): void {
    this.taskService.getAssignedEmployees(this.taskId).subscribe(res => {
      this.employees = res.data.map(e => ({
        id: e.employeeId,
        fullName: e.employeeName
      }));
    });
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting) return;

    const model: WarningAddEditDto = {
      issuedEmployeeId: this.form.value.selectedEmployeeId,
      reason: this.form.value.reason.trim()
    };

    this.isSubmitting = true;

    this.warningService.create(this.taskId, model).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.modal.close(true);
      },
      error: (err) => {
        console.error('Failed to submit warning', err);
        this.isSubmitting = false;
      }
    });
  }
}
