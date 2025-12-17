import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { DiscountAddEditDto } from 'app/core/models/task/task-penalty';
import { TaskPenaltyService } from 'app/core/services/task-penalty.service';
import { EmployeeService } from 'app/core/services/employee.service';
import { Employee } from 'app/core/models/employee/employee';
import { TaskService } from 'app/core/services/task.service';
import { SimpleEmployee } from 'app/core/models/task/task';

@Component({
  selector: 'app-penalty-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  templateUrl: './penalty-modal.component.html'
})
export class PenaltyModalComponent implements OnInit {

  @Input() taskId!: number;

  selectedEmployeeId!: number;

  amount!: number;
  reason = '';
  isSubmitting = false;
  employees: SimpleEmployee[] = [];


  constructor(
    public modal: NgbActiveModal,
    private penaltyService: TaskPenaltyService,
    private employeeService: TaskService
  ) {}

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    const request = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 1000,
      sortColumn: 'Id',
      sortDirection: 'ASC'
    };



    this.employeeService.getAssignedEmployees(this.taskId).subscribe(res => {
this.employees = res.data.map(e => ({
  id: e.employeeId,
  fullName: e.employeeName
}));    });
  }

  submit(): void {
    if (
      !this.selectedEmployeeId ||
      !this.amount ||
      !this.reason.trim() ||
      this.isSubmitting
    ) return;

    const model: DiscountAddEditDto = {
      employeeId: this.selectedEmployeeId,
      amount: this.amount,
      reason: this.reason.trim()
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
