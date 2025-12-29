import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, MinLengthValidator, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { TaskService } from 'app/core/services/task.service';
import { EmployeeService } from 'app/core/services/employee.service';
import { Employee } from 'app/core/models/employee/employee';
import { ToastrService } from 'ngx-toastr';
import { FormFieldConfig } from 'app/core/models/form-field-config';
import { CommentAllowPeriod, TaskPriority, TaskStatus } from 'app/core/models/task/task';

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

  @Input() isEdit: boolean = false;
  @Input() taskId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  formGroup!: FormGroup;

  title = 'TASK.ADD';
  breadcrumbs = ['HOME', 'TASKS'];
  activeitem = 'TASK.ADD';

  formConfig: FormFieldConfig[] = [

    {
      type: 'input',
      label: 'TASK.TITLE',
      name: 'title',
      validations: { required: true, minlength: 3, maxlength: 300 },
      defaultValue: ''
    },

    {
      type: 'select',
      label: 'TASK.ASSIGNED_EMPLOYEES',
      name: 'assignedEmployeeIds',
      selectType: 'employee',
      multiple: true,
      options: [],
      validations: { required: true },
      defaultValue: []
    },

    {
      type: 'select',
      label: 'TASK.PRIORITY',
      name: 'priority',
      selectType: 'simple',
      options: [
        { label: 'TASK.PRIORITY_LOW', value: TaskPriority.Low },
        { label: 'TASK.PRIORITY_MEDIUM', value: TaskPriority.Medium },
        { label: 'TASK.PRIORITY_HIGH', value: TaskPriority.High }
      ],
      validations: { required: true },
      defaultValue: TaskPriority.Low
    },
    {
      type: 'select',
      label: 'TASK.STATUS',
      name: 'status',
      selectType: 'simple',
      options: [
        { label: 'TASK.STATUS_NEW', value: TaskStatus.New },
        { label: 'TASK.STATUS_IN_PROGRESS', value: TaskStatus.InProgress },
        { label: 'TASK.STATUS_CLOSED', value: TaskStatus.Closed },
        { label: 'TASK.STATUS_ARCHIVED', value: TaskStatus.Archived }

      ],
      validations: { required: true },
      defaultValue: TaskStatus.New
    },

    {
      type: 'date',
      label: 'TASK.DUE_DATE',
      name: 'dueDate',
      defaultValue: null
    },

    {
      type: 'select',
      label: 'TASK.COMMENT_ALLOW_PERIOD',
      name: 'commentAllowPeriodDays',
      selectType: 'simple',
      options: [
        { label: 'TASK.DAILY', value: CommentAllowPeriod.Daily },
        { label: 'TASK.WEEKLY', value: CommentAllowPeriod.Weekly },
        { label: 'TASK.MONTHLY', value: CommentAllowPeriod.Monthly },
      ],
    },

    {
      type: 'input',
      inputType: 'number',
      label: 'TASK.MAX_WARNINGS',
      name: 'maxWarnings',
      defaultValue: 3
    },

    {
      type: 'input',
      inputType: 'number',
      label: 'TASK.PENALTY_AT_MAX_WARNINGS',
      name: 'penaltyAtMaxWarnings',
      defaultValue: 0
    },

    {
      type: 'input',
      inputType: 'number',
      label: 'TASK.PENALTY_ON_AUTO_CLOSE',
      name: 'penaltyOnAutoClose',
      defaultValue: 0
    },

    {
      type: 'checkbox',
      label: 'TASK.IS_SHARED',
      name: 'isShared',
      defaultValue: false
    },

    {
      type: 'textarea',
      label: 'TASK.DESCRIPTION',
      name: 'description',
      defaultValue: ''
    }
  ];

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService,
    private employeeService: EmployeeService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) { }

  ngOnInit(): void {
    this.initForm();
    this.loadEmployees();

    if (this.isEdit && this.taskId) {
      this.loadTask();
    }
  }

  initForm() {
    this.formGroup = this.fb.group({
      title: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(100)
      ]],
      description: [''],
      assignedEmployeeIds: [[], Validators.required],
      priority: [TaskPriority.Low, Validators.required],
      status: [TaskStatus.New, Validators.required],
      dueDate: [null],
      commentAllowPeriodDays: ['يومي'],
      maxWarnings: [3, [Validators.pattern('^[0-9]+$')]],
      penaltyAtMaxWarnings: [0, [Validators.pattern('^[0-9]+$')]],
      penaltyOnAutoClose: [0, [Validators.pattern('^[0-9]+$')]],
      isShared: [false]
    });

  }

  loadTask() {
    this.taskService.getById(this.taskId!).subscribe(res => {
      const task = res.data;
      const assignedEmployeeIds =
        (task.assignEmployee || []).map((e: any) => e.id);


      this.formGroup.patchValue({ assignedEmployeeIds });


      let dueDate: string | null = null;
      if (task.dueDate) {
        const d = new Date(task.dueDate);
        const month = String(d.getMonth() + 1).padStart(2, '0');
        const day = String(d.getDate()).padStart(2, '0');
        dueDate = `${d.getFullYear()}-${month}-${day}`;
      }

      const patch = {
        ...task,
        assignedEmployeeIds,
        dueDate
      };

      this.formGroup.patchValue(patch);
    });
  }


  loadEmployees() {
    const request = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 1000,
      sortColumn: 'Id',
      sortDirection: 'ASC'
    };

    this.employeeService.getAll(request).subscribe(res => {
      const list = res.data.data;

      const field = this.formConfig.find(f => f.name === 'assignedEmployeeIds');
      if (field) {
        field.options = list.map((emp: Employee) => ({
          label: emp.fullName,
          value: emp.id,
          mobile: emp.mobile,
          email: emp.email
        }));
      }
    });
  }

  onSubmit(formData: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(
        this.translate.instant('FORM.VALIDATION_ERROR'),
        this.translate.instant('FORM.ERROR')
      );
      return;
    }

    const request$ = this.isEdit && this.taskId
      ? this.taskService.update(this.taskId, this.formGroup.value)
      : this.taskService.create(this.formGroup.value);

    request$.subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('TASK.SAVED_SUCCESS'));
        this.formSubmitted.emit();
      }
    });
  }
}
