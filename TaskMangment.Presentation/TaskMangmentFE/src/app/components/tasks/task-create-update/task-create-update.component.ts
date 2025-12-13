import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Employee } from 'app/core/models/employee/employee';
import { FormFieldConfig } from 'app/core/models/form-field-config';
import { EmployeeService } from 'app/core/services/employee.service';
import { TaskService } from 'app/core/services/task.service';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { Validators } from 'ngx-editor';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-task-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './task-create-update.component.html'
})
export class TaskCreateUpdateComponent implements OnInit {

  @Input() isEdit = false;
  @Input() taskId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  formGroup!: FormGroup;

  formConfig: FormFieldConfig[] = [

    /* ================= TITLE ================= */
    {
      type: 'input',
      inputType: 'text',
      label: 'TASK.TITLE',
      name: 'title',
      validations: { required: true, maxlength: 300 }
    },

    /* ================= ASSIGNED EMPLOYEES ================= */
    {
      type: 'select',
      label: 'TASK.ASSIGNED_EMPLOYEES',
      name: 'assignedEmployeeIds',
      options: [],              // تُملأ من employees
      multiple: true,
      selectType: 'employee',   // مهم للـ template
      validations: { required: true }
    },

    /* ================= PRIORITY ================= */
    {
      type: 'select',
      label: 'TASK.PRIORITY',
      name: 'priority',
      selectType: 'simple',
      options: [
        { label: 'TASK.PRIORITY_LOW', value: 0 },
        { label: 'TASK.PRIORITY_MEDIUM', value: 1 },
        { label: 'TASK.PRIORITY_HIGH', value: 2 }
      ],
      validations: { required: true }
    },

    /* ================= DUE DATE ================= */
    {
      type: 'date',
      label: 'TASK.DUE_DATE',
      name: 'dueDate'
    },

    /* ================= COMMENT PERIOD ================= */
    {
      type: 'input',
      inputType: 'number',
      label: 'TASK.COMMENT_ALLOW_PERIOD',
      name: 'commentAllowPeriodDays'
    },

    /* ================= WARNINGS ================= */
    {
      type: 'input',
      inputType: 'number',
      label: 'TASK.MAX_WARNINGS',
      name: 'maxWarnings'
    },

    /* ================= PENALTIES ================= */
    {
      type: 'input',
      inputType: 'number',
      label: 'TASK.PENALTY_AT_MAX_WARNINGS',
      name: 'penaltyAtMaxWarnings'
    },

    {
      type: 'input',
      inputType: 'number',
      label: 'TASK.PENALTY_ON_AUTO_CLOSE',
      name: 'penaltyOnAutoClose'
    },

    /* ================= SHARED ================= */
    {
      type: 'checkbox',
      label: 'TASK.IS_SHARED',
      name: 'isShared'
    },

    /* ================= DESCRIPTION ================= */
    {
      type: 'textarea',
      label: 'TASK.DESCRIPTION',
      name: 'description'
    }

  ];



  title = 'TASK.ADD';
  breadcrumbs = ['HOME', 'TASKS'];
  activeitem = 'TASK.ADD';

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService,
    private toastr: ToastrService,
    private employeeService: EmployeeService,
    private translate: TranslateService
  ) { }

  ngOnInit() {
    this.formGroup = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      assignedEmployeeIds: [[], Validators.required],
      priority: [0, Validators.required],
      dueDate: [null],
      commentAllowPeriodDays: [null],
      maxWarnings: [3],
      penaltyAtMaxWarnings: [0],
      penaltyOnAutoClose: [0],
      isShared: [false]
    });

    this.loadEmployees();
    if (this.isEdit && this.taskId) {
      this.taskService.getById(this.taskId).subscribe(res => {
        this.formGroup.patchValue(res.data);
      });
    }
  }

  onSubmit() {
    if (this.formGroup.invalid) return;

    const req = this.isEdit
      ? this.taskService.update(this.taskId!, this.formGroup.value)
      : this.taskService.create(this.formGroup.value);

    req.subscribe(() => {
      this.toastr.success(this.translate.instant('TASK.SAVED_SUCCESS'));
      this.formSubmitted.emit();
    });
  }

  loadEmployees() {
    const req = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 1000,
      sortColumn: 'Id',
      sortDirection: 'ASC'
    };

    this.employeeService.getAll(req).subscribe(res => {
      const list = res.data.data;

      const assignedEmployeeIds = this.formConfig.find(x => x.name === 'assignedEmployeeIds');
       
      if (assignedEmployeeIds) {
        assignedEmployeeIds.options = list.map((emp: Employee) => ({
          label: emp.fullName,
          value: emp.id
        }));
      } 
    });
  }
}
