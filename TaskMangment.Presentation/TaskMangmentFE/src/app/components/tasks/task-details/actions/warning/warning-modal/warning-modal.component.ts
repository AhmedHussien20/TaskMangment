import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { TaskService } from 'app/core/services/task.service';
import { SimpleEmployee } from 'app/core/models/task/task';
import { TaskWarningService } from 'app/core/services/task-warning.service';
import { WarningAddEditDto } from 'app/core/models/task/task-warning';

@Component({
  selector: 'app-warning-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './warning-modal.component.html'
})
export class WarningModalComponent implements OnInit {

  @Input() taskId!: number;

  selectedEmployeeId!: number;
  reason = '';
  isSubmitting = false;
  employees: SimpleEmployee[] = [];

  constructor(
    public modal: NgbActiveModal,
    private warningService: TaskWarningService,
    private taskService: TaskService
  ) {}

  ngOnInit(): void {
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
    if (
      !this.selectedEmployeeId ||
      !this.reason.trim() ||
      this.isSubmitting
    ) return;

    const model: WarningAddEditDto = {
      issuedEmployeeId: this.selectedEmployeeId,
      reason: this.reason.trim()
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
