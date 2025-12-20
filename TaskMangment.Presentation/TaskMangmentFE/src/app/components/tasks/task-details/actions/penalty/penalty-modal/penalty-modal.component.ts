import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { TaskPenaltyService } from 'app/core/services/task-penalty.service';
import { TaskService } from 'app/core/services/task.service';
import { DiscountAddEditDto } from 'app/core/models/task/task-penalty';
import { SimpleEmployee } from 'app/core/models/task/task';

@Component({
  selector: 'app-penalty-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './penalty-modal.component.html'
})
export class PenaltyModalComponent implements OnInit {

  @Input() taskId!: number;

  form!: FormGroup;
  isSubmitting = false;
  employees: SimpleEmployee[] = [];

  constructor(
    public modal: NgbActiveModal,
    private fb: FormBuilder,
    private penaltyService: TaskPenaltyService,
    private employeeService: TaskService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      selectedEmployeeId: [null, Validators.required],
      amount: [null, [Validators.required, Validators.min(1)]],
      reason: ['', [Validators.required, Validators.minLength(5)]]
    });

    this.loadEmployees();
  }

  loadEmployees(): void {
    this.employeeService.getAssignedEmployees(this.taskId).subscribe(res => {
      this.employees = res.data.map(e => ({
        id: e.employeeId,
        fullName: e.employeeName
      }));
    });
  }

  submit(): void {
    if (this.form.invalid || this.isSubmitting) return;

    const model: DiscountAddEditDto = {
      employeeId: this.form.value.selectedEmployeeId,
      amount: this.form.value.amount,
      reason: this.form.value.reason.trim()
    };

    this.isSubmitting = true;

    this.penaltyService.create(this.taskId, model).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.modal.close(true);
      },
      error: (err) => {
        console.error('Failed to submit penalty', err);
        this.isSubmitting = false;
      }
    });
  }
}
