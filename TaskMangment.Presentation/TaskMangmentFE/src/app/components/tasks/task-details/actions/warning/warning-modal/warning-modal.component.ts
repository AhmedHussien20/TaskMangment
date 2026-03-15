import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { TaskService } from 'app/core/services/task.service';
import { TaskWarningService } from 'app/core/services/task-warning.service';
import { WarningAddEditDto } from 'app/core/models/task/task-warning';
import { SimpleEmployee } from 'app/core/models/task/task';
import { ToastrService } from 'ngx-toastr';
import { EmployeeNgSelectComponent, EmployeeOption } from 'app/components/employee-select/employee-select.component';

@Component({
  selector: 'app-warning-modal',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule,EmployeeNgSelectComponent],
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
    private taskService: TaskService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  assignedEmployees: EmployeeOption[] = [];


ngOnInit(): void {
  this.form = this.fb.group({
    selectedEmployeeId: [null, Validators.required],
    reason: ['', [Validators.required, Validators.minLength(5)]]
  });
  this.taskService.getAssignedEmployees(this.taskId).subscribe(res => {
    this.assignedEmployees = res.data.map(e => ({
      value: e.employeeId,
      label: e.employeeName,
      email: e.email,
      mobile: e.mobile,
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
        this.toastr.success(this.translate.instant('TASK.WARNING_SUCCESS'));
        this.isSubmitting = false;
        this.modal.close(true);
      }
    });
  }
}
