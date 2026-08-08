import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { LeaveTypeService } from 'app/core/services/leave-type.service';
import { LeaveTypeAddEditDto } from 'app/core/models/leave/leave-type.model';

@Component({
  selector: 'app-leave-type-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './leave-type-create-update.component.html',
  styleUrls: ['./leave-type-create-update.component.scss']
})
export class LeaveTypeCreateUpdateComponent implements OnInit {

  @Input() isEdit: boolean = false;
  @Input() leaveTypeId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

 title = 'LEAVE_TYPE.title';
  activeitem = 'LEAVE_TYPE.title';
  breadcrumbs = ['MENU.HOME', 'MENU.LEAVES', 'LEAVE_TYPE.title'];

  formGroup!: FormGroup;

  formConfig = [
    {
      type: 'input',
      label: 'LEAVE_TYPE.NAME_AR',
      name: 'nameAr',
      validations: { required: true, minlength: 3, maxlength: 100 },
      defaultValue: ''
    },
    {
      type: 'input',
      label: 'LEAVE_TYPE.NAME_EN',
      name: 'nameEn',
      validations: { required: true, minlength: 3, maxlength: 100 },
      defaultValue: ''
    },
    {
      type: 'input',
      label: 'LEAVE_TYPE.MAX_DAYS',
      name: 'maxDaysPerYear',
      validations: { min: 0 },
      defaultValue: null
    },
    {
      type: 'checkbox',
      label: 'LEAVE_TYPE.IS_PAID',
      name: 'isPaid',
      defaultValue: true
    },
  ];

  constructor(
    private fb: FormBuilder,
    private leaveTypeService: LeaveTypeService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) { }

  ngOnInit() {
    this.initForm();

    if (this.isEdit && this.leaveTypeId) {
      this.loadLeaveType();
    }
  }

  initForm() {
    this.formGroup = this.fb.group({
      nameAr: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      nameEn: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      isPaid: [true],
      maxDaysPerYear: [null]
    });
  }

  loadLeaveType() {
    if (!this.leaveTypeId) return;

    this.leaveTypeService.getById(this.leaveTypeId).subscribe(res => {
      const leaveType: LeaveTypeAddEditDto = res.data;
      this.formGroup.patchValue({
        nameAr: leaveType.nameAr,
        nameEn: leaveType.nameEn,
        isPaid: leaveType.isPaid,
        maxDaysPerYear: leaveType.maxDaysPerYear
      });
    });
  }

  onSubmit(formData: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    if (this.isEdit && this.leaveTypeId) {
      // UPDATE
      this.leaveTypeService.update(this.leaveTypeId, this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('LEAVE_TYPE.UPDATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    } else {
      // CREATE
      this.leaveTypeService.add(this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('LEAVE_TYPE.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    }
  }
}
