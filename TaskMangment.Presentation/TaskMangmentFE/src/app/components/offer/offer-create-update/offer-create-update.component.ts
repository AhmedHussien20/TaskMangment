import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';

import { OfferService } from 'app/core/services/offer.service';
import { CourseService } from 'app/core/services/course.service';
import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormFieldConfig } from 'app/core/models/form-field-config';
import { StudentService } from 'app/core/services/student.service';

@Component({
  selector: 'app-offer-create-update',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, GenericFormComponent],
  templateUrl: './offer-create-update.component.html'
})
export class OfferCreateUpdateComponent implements OnInit {

  @Input() isEdit: boolean = false;
  @Input() offerId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  title = 'OFFER.TITLE';
  breadcrumbs = ['HOME', 'OFFERS'];
  activeitem = 'OFFER.CREATE';

  formGroup!: FormGroup;

  formConfig: FormFieldConfig[] = [
    {
     type: 'input',
      label: 'OFFER.TITLE',
      name: 'title',
      validations: { required: true, minlength: 3, maxlength: 200 },
      defaultValue: ''
    },
    {
      type: 'select',
      label: 'OFFER.COURSE',
      name: 'courseId',
      selectType: 'simple',
      options: [],
      validations: { required: true },
      defaultValue: null
    },
    {
      type: 'select',
      label: 'OFFER.SUBJECT',
      name: 'subjectId',
      selectType: 'simple',
      options: [],
      validations: { required: true },
      defaultValue: null
    },
    {
      type: 'date',
      label: 'OFFER.START_DATE',
      name: 'startDate',
      validations: { required: true },
      defaultValue: ''
    },
    {
      type: 'date',
      label: 'OFFER.END_DATE',
      name: 'endDate',
      defaultValue: null
    },
    {
  type: 'select',
  label: 'OFFER.STUDENTS',
  name: 'assignedStudentIds',
  selectType: 'simple',
  multiple: true,
  options: [],
  validations: { required: true },
  defaultValue: []
}

    ,{
  type: 'input',
  label: 'OFFER.PAYMENT_METHOD',
  name: 'paymentMethod',
  validations: { required: true },
  defaultValue: ''
},
{
  type: 'input',
  inputType : 'number',
  label: 'OFFER.PRICE',
  name: 'price',
  validations: { required: true },
  defaultValue: null
},
{
  type: 'input',
  inputType : 'number',
  label: 'OFFER.INTEREST_RATE',
  name: 'interestRate',
  defaultValue: null
},
{
  type: 'input',
  inputType : 'number',
  label: 'OFFER.DISCOUNT_RATE',
  name: 'discountRate',
  defaultValue: null
},
{
  type: 'input',
  inputType : 'number',
  label: 'OFFER.INSTALLMENT_VALUE',
  name: 'installmentValue',
  defaultValue: null
},
{
  type: 'input',
  inputType : 'number',
  label: 'OFFER.NET_AMOUNT',
  name: 'netAmount',
  defaultValue: null
},
{
  type: 'input',
  label: 'OFFER.OFFER_OWNER',
  name: 'offerOwner',
  defaultValue: ''
},
{
  type: 'input',
  label: 'OFFER.SPECIALIZATION',
  name: 'specialization',
  defaultValue: ''
},
 {
      type: 'textarea',
      label: 'OFFER.DESCRIPTION',
      name: 'description',
      defaultValue: ''
    }

  ];

  constructor(
    private fb: FormBuilder,
    private offerService: OfferService,
    private courseService: CourseService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private studentService: StudentService,

  ) {}

  ngOnInit() {
    this.initForm();
    this.loadCourses();
    this.loadStudents();

    this.formGroup.get('courseId')?.valueChanges.subscribe(courseId => {
      this.loadSubjects(courseId);
      this.formGroup.patchValue({ subjectId: null });
    });

    if (this.isEdit && this.offerId) {
      this.activeitem = 'OFFER.UPDATE';
      this.loadOffer();
    }
  }

  initForm() {
  this.formGroup = this.fb.group({
    title: ['', [
      Validators.required,
      Validators.minLength(3),
      Validators.maxLength(200)
    ]],

    courseId: [null, Validators.required],

    subjectId: [null, Validators.required],

    startDate: ['', Validators.required],

    endDate: [null],

    assignedStudentIds: [[], Validators.required],

    paymentMethod: ['', Validators.required],

    price: [null, [
      Validators.required,
      Validators.pattern('^[0-9]+(\\.[0-9]+)?$')
    ]],

    interestRate: [null, [Validators.pattern('^[0-9]+(\\.[0-9]+)?%?$'),Validators.required]],

    discountRate: [null,  [Validators.pattern('^[0-9]+(\\.[0-9]+)?%?$'),Validators.required]],

    installmentValue: [null,  [Validators.pattern('^[0-9]+(\\.[0-9]+)?%?$'),Validators.required]],

    netAmount: [null,  [Validators.pattern('^[0-9]+(\\.[0-9]+)?%?$'),Validators.required]],

    offerOwner: [''],

    specialization: [''],

    description: ['']
  });
}


  loadOffer() {
  if (!this.offerId) return;

  this.offerService.getById(this.offerId).subscribe(res => {
    const o = res.data;

    let startDate: string | null = null;
    if (o.startDate) {
      const d = new Date(o.startDate);
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      startDate = `${d.getFullYear()}-${month}-${day}`;
    }

    let endDate: string | null = null;
    if (o.endDate) {
      const d = new Date(o.endDate);
      const month = String(d.getMonth() + 1).padStart(2, '0');
      const day = String(d.getDate()).padStart(2, '0');
      endDate = `${d.getFullYear()}-${month}-${day}`;
    }

    const patch = {
      ...o,
      startDate,
      endDate
    };

    this.formGroup.patchValue(patch);
    this.formGroup.patchValue({ assignedStudentIds: o.assignedStudentsIds ?? [] });

    if (o.courseId) this.loadSubjects(o.courseId);
  });
}


  loadCourses() {
    const req = { searchKey: '', pageIndex: 1, pageSize: 500, sortColumn: 'Id', sortDirection: 'ASC' };
    this.courseService.getAll(req).subscribe(res => {
  const list: { id: number; title: string }[] = res.data.data;
      const field = this.formConfig.find(f => f.name === 'courseId');
      if (field) field.options = list.map(c => ({ label: c.title, value: c.id }));
    });
  }

  loadSubjects(courseId: number) {
    if (!courseId) return;
    this.courseService.getSubjectsByCourse(courseId).subscribe(res => {
      const list = res.data;
      const field = this.formConfig.find(f => f.name === 'subjectId');
      if (field) field.options = list.map(s => ({ label: s.title, value: s.id }));
    });
  }
  loadStudents() {
  const request = {
    searchKey: '',
    pageIndex: 1,
    pageSize: 1000,
    sortColumn: 'Id',
    sortDirection: 'ASC'
  };

  this.studentService.getAll(request).subscribe(res => {
    const list = res.data?.data;
    if (!Array.isArray(list)) return;

    const field = this.formConfig.find(f => f.name === 'assignedStudentIds');
    if (field) {
      field.options = list.map((s: any) => ({
        label: s.fullName,
        value: s.id
      }));
    }
  });
}


  onSubmit(formValue: any) {
    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    if (this.isEdit && this.offerId) {
      this.offerService.update(this.offerId, this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('OFFER.UPDATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    } else {
      this.offerService.create(this.formGroup.value).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('OFFER.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        }
      });
    }
  }
}
