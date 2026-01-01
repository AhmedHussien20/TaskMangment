import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';

import { StudentService } from 'app/core/services/student.service';

import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormFieldConfig } from 'app/core/models/form-field-config';

@Component({
  selector: 'app-student-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './student-create-update.component.html'
})
export class StudentCreateUpdateComponent implements OnInit {

  @Input() isEdit: boolean = false;
  @Input() studentId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  title = 'STUDENT.TITLE';
  breadcrumbs = ['HOME', 'STUDENTS'];
  activeitem = 'STUDENT.CREATE';

  formGroup!: FormGroup;

  formConfig: FormFieldConfig[] = [
    { 
      type: 'input', 
      label: 'STUDENT.FULL_NAME', 
      name: 'fullName', 
      validations: { required: true, maxlength: 250 }, 
      defaultValue: '' 
    },
    { 
      type: 'input', 
      inputType: 'email',
      label: 'STUDENT.EMAIL', 
      name: 'email', 
      validations: { email: true, maxlength: 200 }, 
      defaultValue: '' 
    },
    { 
      type: 'input', 
      inputType: 'number',
      label: 'STUDENT.MOBILE', 
      name: 'mobile', 
      validations: { maxlength: 50 }, 
      defaultValue: '' 
    }
  ];

  constructor(
    private fb: FormBuilder,
    private studentService: StudentService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) { }

  ngOnInit() {
    this.initForm();
    if (this.isEdit && this.studentId) {
      this.loadStudent();
    }
  }

  initForm() {
    this.formGroup = this.fb.group({
      fullName: ['', Validators.required],
      email: ['', [Validators.email,Validators.required]],
      mobile: ['',Validators.pattern('^\\+?[0-9]+$')]
    });
  }

  loadStudent() {
    if (!this.studentId) return;

    this.studentService.getById(this.studentId).subscribe(res => {
      const student = res.data;

      this.formGroup.patchValue({
        fullName: student.fullName,
        email: student.email,
        mobile: student.mobile
      });
    });
  }

  onSubmit(formValue: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    // UPDATE
    if (this.isEdit && this.studentId) {
      this.studentService.update(this.studentId, this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('STUDENT.UPDATE_SUCCESS'));
          this.formSubmitted.emit();
        },
        error: () => {
          this.toastr.error(this.translate.instant('STUDENT.UPDATE_FAILED'));
        }
      });
    }

    // CREATE
    else {
      this.studentService.create(this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('STUDENT.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        },
        error: () => {
          this.toastr.error(this.translate.instant('STUDENT.CREATE_FAILED'));
        }
      });
    }
  }
}