import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';

@Component({
  selector: 'app-task-basic-info',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    GenericFormComponent
  ],
  templateUrl: './task-basic-info.component.html'
})
export class TaskBasicInfoComponent implements OnInit {

  @Input() taskId!: number;
  @Input() readonly = false;

  form!: FormGroup;

  taskInfo: any;

  formConfig = [
    { name: 'title', label: 'عنوان المهمة', type: 'text' },
    { name: 'description', label: 'تفاصيل المهمة', type: 'textarea' },
    {
      name: 'commentPeriod',
      label: 'فترة السماح للتعليق',
      type: 'text'
    },
    { name: 'maxWarnings', label: 'الحد الأقصى للتحذيرات', type: 'number' },
    { name: 'startDate', label: 'تاريخ الإنشاء', type: 'date' },
    { name: 'endDate', label: 'تاريخ التسليم', type: 'date' }
  ];

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.buildForm();
    this.loadTask();
  }

  buildForm() {
    this.form = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      commentPeriod: [''],
      maxWarnings: [0],
      startDate: [null],
      endDate: [null]
    });
  }

  loadTask() {
    //  replace with API
    this.taskInfo = {
      id: 19353,
      title: 'مشروع دبي أبو وشاح',
      description: 'تذكير بمشروع دبي أبو وشاح',
      commentPeriod: 'يومي',
      maxWarnings: 3,
      warningPenalty: 'SAR 0',
      autoClosePenalty: 'SAR 0',
      startDate: '2025-11-09 12:05 PM',
      endDate: '2026-01-01 01:59 AM',
      assignedTo: 'المهندس وسام أبو خضر'
    };

    this.form.patchValue(this.taskInfo);
  }

  onSubmit() {
    if (this.form.invalid) return;
    console.log('Save basic info:', this.form.value);
  }
}
