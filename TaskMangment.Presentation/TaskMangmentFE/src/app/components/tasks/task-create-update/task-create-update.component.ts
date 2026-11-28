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
import { decimalValidator, penaltyAmountValidator } from 'app/shared/validations/numberVlidator';
import {
  notPastDueDateValidator,
  startOfToday,
  taskDueDateCalendarFilter,
  weekendDueDateValidator
} from 'app/shared/validations/weekend-due-date.validator';

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
  @Input() isCopy: boolean = false;
  /** Preselect assignees when creating a task from Employee 360. */
  @Input() preselectedEmployeeIds: number[] = [];
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
      isPaginated: true,
      searchFunction: (searchTerm: string) => {
          const request = {
            searchKey: searchTerm || '',
            pageIndex: 1,
            pageSize: 20, 
            sortColumn: 'Id',
            sortDirection: 'DESC'
          };
          
          return this.employeeService.getAll(request);
        },
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
      defaultValue: TaskStatus.New,
  disabled: !this.isEdit 

    },

    {
      type: 'date',
      label: 'TASK.DUE_DATE',
      name: 'dueDate',
      defaultValue: null,
      minDate: startOfToday(),
      dateFilter: taskDueDateCalendarFilter,
      errorMessages: {
        weekendDueDate: 'TASK.DUE_DATE_WEEKEND',
        pastDueDate: 'TASK.DUE_DATE_PAST'
      }
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
      defaultValue: CommentAllowPeriod.Daily
    },
    {
      type: 'input',
      inputType: 'number',
      label: 'TASK.MIN_COMMENTS_PER_PERIOD',
      name: 'minCommentsPerPeriod',
      validations: { required: true, min: 1, max: 30 },
      defaultValue: 1
    },

    {
      type: 'input',
      inputType: 'number',
      label: 'TASK.MAX_WARNINGS_BEFORE_DISCOUNT',
      name: 'maxWarningsBeforeDiscount',
      validations: { required: true, min: 0, max: 3 },
      defaultValue: 3
    },

    {
      type: 'input',
      inputType: 'number',
      label: 'TASK.PENALTY_ON_AUTO_CLOSE',
      name: 'penaltyOnAutoClose',
      validations: { required: true, minPenalty: 50 },
      defaultValue: null
    },
    {
      type: 'input',
      inputType: 'number',
      label: 'TASK.PENALTY_ON_STOP_COMMENT',
      name: 'penaltyOnStopComment',
      validations: { required: true, minPenalty: 50 },
      defaultValue: null
    },
    {
      type: 'checkbox',
      label: 'TASK.IS_SHARED',
      name: 'isShared',
      defaultValue: false
    },
    {
      type: 'checkbox',
      label: 'TASK.REQUIRE_UPLOAD_FILE_WHEN_COMMENTING',
      name: 'requireUploadFile',
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

  if ((this.isEdit || this.isCopy) && this.taskId) {
    this.loadTask();
  } else if (!this.isEdit && this.preselectedEmployeeIds?.length) {
    this.applyPreselectedEmployees();
  }
}

  private applyPreselectedEmployees(): void {
    const ids = [...this.preselectedEmployeeIds];
    this.formGroup.patchValue({ assignedEmployeeIds: ids });

    const field = this.formConfig.find(f => f.name === 'assignedEmployeeIds');
    if (!field) return;

    // Ensure preselected employees appear in options even if not on first page.
    ids.forEach(id => {
      const exists = (field.options || []).some((o: any) => o.value === id);
      if (!exists) {
        this.employeeService.getById(id).subscribe({
          next: (res: any) => {
            const emp = res?.data;
            if (!emp) return;
            field.options = [
              {
                label: emp.fullName,
                value: emp.id ?? id,
                mobile: emp.mobile,
                email: emp.email
              },
              ...(field.options || [])
            ];
            this.formGroup.patchValue({ assignedEmployeeIds: ids });
          }
        });
      }
    });
  }

  initForm() {
    this.formGroup = this.fb.group({
      title: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(100)
      ]],
      description: ['', Validators.required],
      assignedEmployeeIds: [[], Validators.required],
      priority: [TaskPriority.Low, Validators.required],
    status: [{ value: TaskStatus.New, disabled: !this.isEdit }, Validators.required], 
      dueDate: [null, [Validators.required, weekendDueDateValidator(), notPastDueDateValidator()]],
      commentAllowPeriodDays: [CommentAllowPeriod.Daily, Validators.required],
      minCommentsPerPeriod: [1, [Validators.required, Validators.min(1), Validators.max(30)]],
      maxWarningsBeforeDiscount: [3, [Validators.required, Validators.min(0), Validators.max(3)]],
      penaltyOnAutoClose: [50, [Validators.required, decimalValidator(), penaltyAmountValidator()]],
      penaltyOnStopComment : [50, [Validators.required, decimalValidator(), penaltyAmountValidator()]],
      isShared: [false],
      requireUploadFile: [false]
    });

  }

 loadTask() {
  this.taskService.getById(this.taskId!).subscribe(res => {
    const task = res.data;

    const assigned = (task.assignEmployee || []).map((e: any) => ({
      value: e.id,
      label: e.name
    }));

    const assignedEmployeeIds = assigned.map(x => x.value);

    const field = this.formConfig.find(f => f.name === 'assignedEmployeeIds');
    if (field) {
      field.options = field.options || [];
      const existing = new Set(field.options.map((x: any) => x.value));
      const missing = assigned.filter(x => !existing.has(x.value));
      field.options = [...missing, ...field.options];
    }

    let dueDate: string | null = null;
    if (task.dueDate) {
      const d = new Date(task.dueDate);
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      dueDate = `${d.getFullYear()}-${month}-${day}`;
    }

    this.formGroup.patchValue({
      ...task,
      assignedEmployeeIds,
      dueDate,
      
    });
    if (this.isCopy) {
  this.formGroup.patchValue({
    status: TaskStatus.New,
    title: `${task.title} - ${this.translate.instant('TASK.COPY')}`,
    dueDate: null
  });
}
    if (this.isEdit) {
  this.formGroup.get('status')!.enable();
}

  });
}



  loadEmployees() {
  const field = this.formConfig.find(f => f.name === 'assignedEmployeeIds');
  if (field) {
    field.isPaginated = true;
    field.searchFunction = (searchTerm: string, page: number) => {
      const request = {
        searchKey: searchTerm || '',
        pageIndex: page,
        pageSize: 20,
        sortColumn: 'Id',
        sortDirection: 'DESC',
        permissionCode : 'CREATE_TASK'
      };
      
      return this.employeeService.getAll(request);
    };
    
    if (field.searchFunction) {
      field.searchFunction('', 1).subscribe(res => {
        field.options = res.data.data.map((emp: Employee) => ({
          label: emp.fullName,
          value: emp.id,
          mobile: emp.mobile,
          email: emp.email
        }));

        if (!this.isEdit && this.preselectedEmployeeIds?.length) {
          this.applyPreselectedEmployees();
        }
      });
    }
  }
}
  onSubmit(formData: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      const dueCtrl = this.formGroup.get('dueDate');
      if (dueCtrl?.hasError('weekendDueDate')) {
        this.toastr.error(this.translate.instant('TASK.DUE_DATE_WEEKEND'));
      } else if (dueCtrl?.hasError('pastDueDate')) {
        this.toastr.error(this.translate.instant('TASK.DUE_DATE_PAST'));
      } else {
        this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      }
      return;
    }
    const payload = { ...this.formGroup.value, penaltyAtMaxWarnings: 0, maxWarnings: 3 };
    if (payload.dueDate instanceof Date) {
      const d: Date = payload.dueDate;
      const y = d.getFullYear();
      const m = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      payload.dueDate = `${y}-${m}-${day}`;
    }
    const request$ =
  this.isEdit && !this.isCopy && this.taskId
    ? this.taskService.update(this.taskId, payload)
    : this.taskService.create(payload);

    request$.subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('TASK.SAVED_SUCCESS'));
        this.formSubmitted.emit();
      }
    });
  }
}
