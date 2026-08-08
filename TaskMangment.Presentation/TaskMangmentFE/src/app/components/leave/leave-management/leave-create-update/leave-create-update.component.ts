import {Component,EventEmitter,Input,Output,OnInit} from '@angular/core';
import { CommonModule } from '@angular/common';
import {FormBuilder,FormGroup,ReactiveFormsModule,Validators} from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { FormFieldConfig } from 'app/core/models/form-field-config';

import { LeaveService } from 'app/core/services/leave.service';
import { LeaveTypeService } from 'app/core/services/leave-type.service';

@Component({
  selector: 'app-leave-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './leave-create-update.component.html',
  styleUrls: ['./leave-create-update.component.scss']
})
export class LeaveCreateUpdateComponent implements OnInit {

  @Input() isEdit: boolean = false;
  @Input() leaveId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  title = 'LEAVE.title';
  breadcrumbs = ['Home', 'Leaves'];
  activeitem = 'LEAVE.create';

  formGroup!: FormGroup;

  formConfig: FormFieldConfig[] = [
    {
      type: 'select',
      label: 'LEAVE.TYPE',
      name: 'leaveTypeId',
      options: [],
      validations: { required: true },
      defaultValue: null
    },
    {
      type: 'date',
      label: 'LEAVE.START_DATE',
      name: 'startDate',
      validations: { required: true },
      defaultValue: ''
    },
    {
      type: 'date',
      label: 'LEAVE.END_DATE',
      name: 'endDate',
      validations: { required: true },
      defaultValue: ''
    },
    {
      type: 'textarea',
      label: 'LEAVE.NOTES',
      name: 'notes',
      validations: { maxlength: 500 },
      defaultValue: ''
    }
  ];

  constructor(
    private fb: FormBuilder,
    private leaveService: LeaveService,
    private leaveTypeService: LeaveTypeService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  /* ================= Lifecycle ================= */
  ngOnInit(): void {
    this.initForm();
    this.loadLeaveTypes();

    if (this.isEdit && this.leaveId) {
      this.loadLeave();
    }
  }

  /* ================= Form Init ================= */
  initForm(): void {
    this.formGroup = this.fb.group({
      leaveTypeId: [null, Validators.required],
      startDate: ['', Validators.required],
      endDate: ['', Validators.required],
      notes: ['']
    });
  }

  /* ================= Load Leave Types ================= */
  loadLeaveTypes(): void {
    this.leaveTypeService.getAll().subscribe(res => {
      const types = res.data;

      const leaveTypeField = this.formConfig.find(
        f => f.name === 'leaveTypeId'
      );

      if (leaveTypeField) {
        leaveTypeField.options = types.map(t => ({
          label: t.nameAr,
          value: t.id
        }));
      }
    });
  }

  /* ================= Load Leave (Edit Mode) ================= */
  loadLeave(): void {
    if (!this.leaveId) return;

    this.leaveService.getById(this.leaveId).subscribe(res => {
      const leave = res.data;

      this.formGroup.patchValue({
        leaveTypeId: leave.leaveTypeId,
        startDate: leave.startDate,
        endDate: leave.endDate,
        notes: leave.notes
      });
    });
  }

  /* ================= Submit ================= */
 onSubmit(): void {
  if (this.formGroup.invalid) {
    this.formGroup.markAllAsTouched();
    this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
    return;
  }

  if (this.isEdit && this.leaveId) {
   
  } else {
    this.leaveService.create(this.formGroup.value).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('LEAVE.CREATE_SUCCESS'));
        this.formSubmitted.emit();
      }
    });
  }
}
}