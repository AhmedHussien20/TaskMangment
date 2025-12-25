import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, MinLengthValidator, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';

import { EmployeeService } from 'app/core/services/employee.service';
import { BranchService } from 'app/core/services/branch.service';

import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormFieldConfig } from 'app/core/models/form-field-config';

@Component({
  selector: 'app-employee-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './employee-create-update.component.html'
})
export class EmployeeCreateUpdateComponent implements OnInit {

  @Input() isEdit: boolean = false;
  @Input() employeeId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  branches: any[] = [];
  roles: any[] = [];

  title = 'EMPLOYEE.TITLE';
  breadcrumbs = ['HOME', 'EMPLOYEES'];
  activeitem = 'EMPLOYEE.CREATE';

  formGroup!: FormGroup;

  formConfig: FormFieldConfig[] = [
    { 
      type: 'input', 
      label: 'EMPLOYEE.FULL_NAME', 
      name: 'fullName', 
      defaultValue: '' 
    },
    { 
      type: 'select', 
      label: 'EMPLOYEE.BRANCH', 
      selectType: 'simple',
      name: 'branchId', 
      options: [], 
      validations: { required: false } 
    },
    { 
      type: 'input', 
      label: 'EMPLOYEE.TITLE', 
      name: 'title', 
      defaultValue: '' 
    },
    { 
      type: 'input', 
      label: 'EMPLOYEE.NATIONALITY', 
      name: 'nationality', 
      defaultValue: '' 
    },
    { 
      type: 'input', 
      inputType: 'number',
      label: 'EMPLOYEE.IDENTITY_NUMBER', 
      name: 'identityNumber', 
      defaultValue: '' 
    },
    { 
      type: 'input', 
      inputType: 'number',
      label: 'EMPLOYEE.MOBILE', 
      name: 'mobile', 
      defaultValue: '' 
    },
    { 
      type: 'input', 
      label: 'EMPLOYEE.ADDRESS', 
      name: 'address', 
      defaultValue: '' 
    },
    { 
      type: 'input', 
      label: 'EMPLOYEE.QUALIFICATION', 
      name: 'qualification', 
      defaultValue: '' 
    },
    /*{ 
      type: 'select', 
      label: 'EMPLOYEE.ROLES', 
      selectType: 'simple',
      name: 'roleIds', 
      multiple: true,
      options: [], 
      validations: { required: true } 
    },*/
    { 
      type: 'input', 
      inputType: 'email',
      label: 'EMPLOYEE.EMAIL', 
      name: 'email', 
      defaultValue: '' 
    },
    { 
      type: 'input', 
      inputType: 'password',
      label: 'EMPLOYEE.PASSWORD', 
      name: 'password', 
      validations: { required: !this.isEdit, maxlength: 500 }, 
      defaultValue: '',
      showPassword: false
 
    },
     { 
      type: 'input', 
      inputType: 'password',
      label: 'EMPLOYEE.CONFIRM_PASSWORD', 
      name: 'confirmPassword', 
      validations: { required: !this.isEdit, maxlength: 500 , equalto: 'password' }, 
      defaultValue: '',
      showPassword: false
    },
    {
    type: 'file',
    name: 'attachments',
    label: 'FORM.ATTACHMENTS',
    multiple: true,
    accept: 'image/*,.pdf',
    maxFiles: 5
  }
  ];

  constructor(
    private fb: FormBuilder,
    private employeeService: EmployeeService,
    private branchService: BranchService,
   // private roleService: RoleService,
    private toastr: ToastrService,
    private translate: TranslateService
  ) { }

  ngOnInit() {
    this.initForm();
    this.loadBranches();
   // this.loadRoles();
    if (this.isEdit && this.employeeId) {
      this.loadEmployee();
    }
  }

  initForm() {
  this.formGroup = this.fb.group({
    fullName: ['', [
      Validators.required,
      Validators.minLength(3),
      Validators.maxLength(250)
    ]],

    branchId: [null],

    title: ['', [
      Validators.minLength(3),
      Validators.maxLength(50)
    ]],

    nationality: ['', Validators.maxLength(100)],

    identityNumber: ['', [
      Validators.maxLength(100),
      Validators.pattern('^[0-9]+$')
    ]],

    mobile: ['', [
      Validators.maxLength(50),
      Validators.pattern('^[0-9]+$')
    ]],

    address: ['', Validators.maxLength(500)],

    qualification: ['', Validators.maxLength(200)],

    email: ['', [
      Validators.email,
      Validators.maxLength(200),
       Validators.email
    ]],

    password: [''],
    confirmPassword: [''],
    attachments: [null]

  }, {
    validators: this.passwordMatchValidator
  });

  if (!this.isEdit) {
    this.formGroup.get('password')?.setValidators([
      Validators.required,
      Validators.maxLength(500)
    ]);

    this.formGroup.get('confirmPassword')?.setValidators([
      Validators.required,
      Validators.maxLength(500)
    ]);
  }

  this.formGroup.get('password')?.updateValueAndValidity();
  this.formGroup.get('confirmPassword')?.updateValueAndValidity();
}


 passwordMatchValidator(formGroup: FormGroup) {
  const password = formGroup.get('password');
  const confirmPassword = formGroup.get('confirmPassword');

  if (!password || !confirmPassword) return null;

  if (password.value !== confirmPassword.value) {
    confirmPassword.setErrors({
      ...confirmPassword.errors,
      passwordMismatch: true
    });
  } else {
    if (confirmPassword.errors) {
      delete confirmPassword.errors['passwordMismatch'];
      if (Object.keys(confirmPassword.errors).length === 0) {
        confirmPassword.setErrors(null);
      }
    }
  }

  return null;
}


  loadEmployee() {
    if (!this.employeeId) return;

    this.employeeService.getById(this.employeeId).subscribe(res => {
      if (!res) return;
      const emp = res.data;
      this.formGroup.patchValue({
        fullName: emp.fullName,
        branchId: emp.branchId,
        title: emp.title,
        nationality: emp.nationality,
        identityNumber: emp.identityNumber,
        mobile: emp.mobile,
        address: emp.address,
        qualification: emp.qualification,
        roleIds: emp.roleIds || [],
        email: emp.email,
        

        password: '', 
        confirmPassword: '',
        

      });
    });
  }

loadBranches() {
  const req = {
    searchKey: '',
    pageIndex: 1,
    pageSize: 500,
    sortColumn: 'Id',
    sortDirection: 'ASC'
  };

  this.branchService.getAll(req).subscribe(res => {
    const list: { id: number; name: string }[] = res.data.data;

    const options = list.map(b => ({
      label: b.name,
      value: b.id.toString()  
    }));

    const field = this.formConfig.find(x => x.name === 'branchId');
    if (field) {
      field.options = options;
    }

    if (this.isEdit && this.employeeId) {
      const branchId = this.formGroup.get('branchId')?.value?.toString();

      const selected = options.find(o => o.value === branchId);
      if (selected) {
        this.formGroup.get('branchId')?.setValue(selected.value);
      }
    }
  });
}



 /* loadRoles() {
    // Assuming you have a RoleService with getAll method
    const req = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 500,
      sortColumn: 'Id',
      sortDirection: 'ASC'
    };

  
  }*/

 onSubmit(formValue: any) {
  if (this.formGroup.invalid) {
    this.formGroup.markAllAsTouched();
    this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
    return;
  }

  const formData = new FormData();

  Object.keys(formValue).forEach(key => {
    if (key !== 'attachments' && formValue[key] !== null && formValue[key] !== undefined) {
      formData.append(key, formValue[key]);
    }
  });

  const files = formValue.attachments;
  if (files) {
    if (Array.isArray(files)) {
      files.forEach((file: File) => {
        formData.append('attachments', file);
      });
    } else {
      formData.append('attachments', files);
    }
  }

  formData.delete('confirmPassword');

  if (this.isEdit && this.employeeId) {
    if (!formValue.password) {
      formData.delete('password');
    }

    this.employeeService.update(this.employeeId, formData).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('EMPLOYEE.UPDATE_SUCCESS'));
        this.formSubmitted.emit();
      }
    });
  }

  else {
    this.employeeService.create(formData).subscribe({
      next: () => {
        this.toastr.success(this.translate.instant('EMPLOYEE.CREATE_SUCCESS'));
        this.formSubmitted.emit();
      }
    });
  }
} 
}