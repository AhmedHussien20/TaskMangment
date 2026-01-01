import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core'; 
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { AreaService } from 'app/core/services/area.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { EmployeeService } from '../../../core/services/employee.service';
import { Employee } from 'app/core/models/employee/employee';
import { ToastrService } from 'ngx-toastr'; 
import { FormFieldConfig } from 'app/core/models/form-field-config';

export interface SelectOption {
  label: string;
  value: number;
  mobile?: string;
  email?: string;
}
 

@Component({
  selector: 'app-area-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './area-create-update.component.component.html'
})

export class AreaCreateUpdateComponent implements OnInit {
  employees: Employee[] = [];

  @Input() isEdit: boolean = false;
  @Input() areaId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  title = 'AREA.title';
  breadcrumbs = ['Home', 'Areas'];
  activeitem = 'AREA.create';

  formGroup!: FormGroup;

  formConfig: FormFieldConfig[] = [
    {
      type: 'input',
      label: 'AREA.NAME',
      name: 'name',
      validations: { required: true, minlength: 3, maxlength: 100 },
      defaultValue: ''
    },
    {
      type: 'input',
      label: 'AREA.ADDRESS',
      name: 'address',
      validations: {required: true, maxlength: 200 },
      defaultValue: ''
    },
    {
      type: 'select',
      label: 'AREA.MANAGER',
      name: 'managerEmployeeId',
      selectType: 'employee',
      options: [],
      validations: { required: true },
      defaultValue: null
    }
  ];

  constructor(
    private fb: FormBuilder,
    private areaService: AreaService, 
    private employeeService: EmployeeService,
    private toastr: ToastrService,
    private translate: TranslateService

  ) { }

  ngOnInit() {
    this.initForm();
    this.loadEmployees();

    if (this.isEdit && this.areaId) {
      this.loadArea();
    }
  }

  initForm() {
    this.formGroup = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      address: ['', [Validators.required, Validators.maxLength(200)]],
      managerName: ['', Validators.maxLength(100)],
      managerEmployeeId: [null, Validators.required]

    });
  }

  loadArea() {
    if (!this.areaId) return;

    this.areaService.getById(this.areaId).subscribe(res => {
      if (!res) return;
      const area = res.data;
      this.formGroup.patchValue({
        name: area.name,
        address: area.address,
        managerEmployeeId: area.managerEmployeeId
      });

      console.log('Loaded area data: ', res);
    });
  }

  loadEmployees() {
    const request = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 1000,
      sortColumn: 'Id',
      sortDirection: 'DESC'
    };

    this.employeeService.getAll(request).subscribe(res => {
      const list = res.data.data;

      const managerField = this.formConfig.find(f => f.name === 'managerEmployeeId');

       if (managerField) {
    managerField.options = list.map((emp: Employee) => ({
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
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    // ----------- UPDATE MODE -----------
    if (this.isEdit && this.areaId) {
      this.areaService.update(this.areaId, this.formGroup.value).subscribe({
        next: (response) => {
          this.toastr.success(this.translate.instant('AREA.UPDATED_SUCCESS'));
          this.formSubmitted.emit();
        },
      });
    }

    // ----------- CREATE MODE -----------
    else {
      this.areaService.create(this.formGroup.value).subscribe({
        next: (response) => {
          this.toastr.success(this.translate.instant('AREA.CREATE_SUCCESS'));
          this.formSubmitted.emit();
        },
      });
    }
  }

}