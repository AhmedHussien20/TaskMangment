import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';

import { CourseService } from 'app/core/services/course.service';

import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormFieldConfig } from 'app/core/models/form-field-config';

@Component({
  selector: 'app-course-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './course-create-update.component.html'
})
export class CourseCreateUpdateComponent implements OnInit {

  @Input() isEdit: boolean = false;
  @Input() courseId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  title = 'COURSE.TITLE';
  breadcrumbs = ['HOME', 'COURSES'];
  activeitem = 'COURSE.CREATE';

  formGroup!: FormGroup;
  subjects: string[] = [];

  formConfig: FormFieldConfig[] = [
    { 
      type: 'input', 
      label: 'COURSE.TITLE', 
      name: 'title', 
      validations: { required: true, maxlength: 250 }, 
      defaultValue: '' 
    },
    { 
      type: 'textarea', 
      label: 'COURSE.DESCRIPTION', 
      name: 'description', 
      validations: { maxlength: 1000 }, 
      defaultValue: '' 
    }
  ];

  constructor(
    private fb: FormBuilder,
    private courseService: CourseService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) { }

  ngOnInit() {
    this.initForm();
    if (this.isEdit && this.courseId) {
      this.loadCourse();
    }
  }

  initForm() {
    this.formGroup = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      newSubject: [''],
      subjects: [[]]
    });
  }

  loadCourse() {
    if (!this.courseId) return;

    this.courseService.getById(this.courseId).subscribe(res => {
      const course = res.data;

      this.formGroup.patchValue({
        title: course.title,
        description: course.description,
        subjects: course.subjects || []
      });
      this.subjects = course.subjects || [];
    });
  }

  addSubject() {
  const newSubject = this.formGroup.get('newSubject')?.value?.trim();
  
  if (!newSubject) {
    this.toastr.warning(this.translate.instant('COURSE.SUBJECT_REQUIRED'));
    return;
  }

  if (this.subjects.includes(newSubject)) {
    this.toastr.warning(this.translate.instant('COURSE.SUBJECT_EXISTS'));
    return;
  }

  this.subjects.push(newSubject);
  this.formGroup.patchValue({ subjects: this.subjects });
  this.formGroup.get('newSubject')?.setValue('');
}

removeSubject(index: number) {
  this.subjects.splice(index, 1);
  this.formGroup.patchValue({ subjects: this.subjects });
}


  onSubmit(formValue: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    // Prepare data
    const submitData = {
      title: this.formGroup.value.title,
      description: this.formGroup.value.description,
      subjects: this.subjects
    };

    // UPDATE
    if (this.isEdit && this.courseId) {
      this.courseService.update(this.courseId, submitData).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('COURSE.UPDATE_SUCCESS'));
          this.formSubmitted.emit();
        },
        error: () => {
          this.toastr.error(this.translate.instant('COURSE.UPDATE_FAILED'));
        }
      });
    }

    // CREATE
    else {
      this.courseService.create(submitData).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('COURSE.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        },
        error: () => {
          this.toastr.error(this.translate.instant('COURSE.CREATE_FAILED'));
        }
      });
    }
  }
}