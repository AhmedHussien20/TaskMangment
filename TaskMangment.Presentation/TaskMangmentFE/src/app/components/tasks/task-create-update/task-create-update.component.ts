import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
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

formConfig = [
  {
    type: 'input',
    label: 'TASK.TITLE',
    name: 'title',
    validations: { required: true, maxlength: 300 }
  },



  {
    type: 'select',
    label: 'TASK.ASSIGNED_EMPLOYEES',
    name: 'assignedEmployeeIds',
    options: [],                 // هتتملي من employees
    multiple: true,
    validations: { required: true }
  },

  {
    type: 'select',
    label: 'TASK.PRIORITY',
    name: 'priority',
    options: [
      { label: 'TASK.PRIORITY_LOW', value: 0 },
      { label: 'TASK.PRIORITY_MEDIUM', value: 1 },
      { label: 'TASK.PRIORITY_HIGH', value: 2 }
    ],
    validations: { required: true }
  },

  {
    type: 'date',
    label: 'TASK.DUE_DATE',
    name: 'dueDate'
  },

  {
    type: 'input',
    label: 'TASK.COMMENT_ALLOW_PERIOD',
    name: 'commentAllowPeriodDays',
    inputType: 'number'
  },

  {
    type: 'input',
    label: 'TASK.MAX_WARNINGS',
    name: 'maxWarnings',
    inputType: 'number'
  },

  {
    type: 'input',
    label: 'TASK.PENALTY_AT_MAX_WARNINGS',
    name: 'penaltyAtMaxWarnings',
    inputType: 'number'
  },

  {
    type: 'input',
    label: 'TASK.PENALTY_ON_AUTO_CLOSE',
    name: 'penaltyOnAutoClose',
    inputType: 'number'
  },

  {
    type: 'checkbox',
    label: 'TASK.IS_SHARED',
    name: 'isShared'
  },
    {
    type: 'textarea',
    label: 'TASK.DESCRIPTION',
    name: 'description',
    multiple: true,
  },
];

  
title = 'TASK.ADD';
breadcrumbs = ['HOME', 'TASKS'];
activeitem = 'TASK.ADD';

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

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
}
