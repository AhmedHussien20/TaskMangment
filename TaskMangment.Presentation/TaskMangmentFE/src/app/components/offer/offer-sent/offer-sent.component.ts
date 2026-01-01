import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Output, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { OfferService } from 'app/core/services/offer.service';
import { StudentService } from 'app/core/services/student.service';
import { ToastrService } from 'ngx-toastr';
import { FormFieldConfig } from 'app/core/models/form-field-config';
import { Student } from 'app/core/models/student/student';
import { OfferAssignStudents } from 'app/core/models/course/offers-course';

@Component({
  selector: 'app-offer-sent',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, GenericFormComponent],
  templateUrl: './offer-sent.component.html',
  styleUrls: ['./offer-sent.component.scss']
})
export class OfferSentComponent implements OnInit {

  @Output() formSubmitted = new EventEmitter<void>();

  formGroup!: FormGroup;

  title = 'OFFER.ASSIGN_TITLE';
  breadcrumbs = ['HOME', 'OFFERS'];
  activeitem = 'OFFER.ASSIGN';

  formConfig: FormFieldConfig[] = [
    {
      type: 'select',
      label: 'OFFER.SELECT_OFFER',
      name: 'offerId',
      selectType: 'simple',
      options: [],
      validations: { required: true },
      defaultValue: null
    },
    {
      type: 'select',
      label: 'OFFER.SELECT_STUDENTS',
      name: 'studentIds',
      selectType: 'simple',
      multiple: true,
      options: [],
      validations: { required: true },
      defaultValue: []
    }
  ];

  constructor(
    private fb: FormBuilder,
    private offerService: OfferService,
    private studentService: StudentService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  ngOnInit(): void {
    this.initForm();
    this.loadOffers();
    this.loadStudents();
  }

  initForm() {
    this.formGroup = this.fb.group({
      offerId: [null, Validators.required],
      studentIds: [[], Validators.required]
    });
  }

 loadOffers() {
     const req = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 500,
      sortColumn: 'Id',
      sortDirection: 'DESC'
    };


  this.offerService.getAll(req).subscribe(res => {
      const list = res.data.data;
      const field =  this.formConfig.find(x => x.name === 'offerId');
      if (field) {
        field.options = list.map(a => ({
          label: a.title,
          value: a.id
        }));
      }
    });
}

loadStudents() {
  const request = {
    searchKey: '',
    pageIndex: 1,
    pageSize: 1000,
    sortColumn: 'Id',
    sortDirection: 'DESC'
  };

  this.studentService.getAll(request).subscribe(res => {

    console.log('FULL RESPONSE', res);
    console.log('res.data', res.data);

    const list = res.data?.data; // ✅ الـ array الحقيقي

    if (!Array.isArray(list)) {
      console.error('Students list is not array', list);
      return;
    }

    const field = this.formConfig.find(f => f.name === 'studentIds');
    if (field) {
      field.options = list.map((s: Student) => ({
        label: s.fullName,
        value: s.id
      }));
    }
  });
}

  onSubmit() {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    const { offerId, studentIds } = this.formGroup.value;

    this.offerService.assignToStudents(offerId, { studentIds } as OfferAssignStudents).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('OFFER.ASSIGN_SUCCESS'));
        this.formSubmitted.emit();
      },
      error: err => {
        this.toastr.error(this.translate.instant('FORM.ERROR'));
        console.error(err);
      }
    });
  }
}
