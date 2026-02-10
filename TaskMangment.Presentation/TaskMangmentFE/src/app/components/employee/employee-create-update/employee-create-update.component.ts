import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, MinLengthValidator, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';

import { EmployeeService } from 'app/core/services/employee.service';
import { BranchService } from 'app/core/services/branch.service';

import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormFieldConfig } from 'app/core/models/form-field-config';
import { DepartmentService } from 'app/core/services/department.service';
import { JobService } from 'app/core/services/job.service';
import { EnumItemDto } from 'app/core/models/employee/employee';

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
  functionCodes: EnumItemDto[] = [];


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
      type: 'select', 
      label: 'EMPLOYEE.JOB', 
      selectType: 'simple',
      name: 'jobId', 
      options: [] 
    },{ 
      type: 'select', 
      label: 'EMPLOYEE.DEPARTMENT', 
      selectType: 'simple',
      name: 'departmentId', 
      options: []
    },
    { 
      type: 'input', 
      label: 'EMPLOYEE.JOB_TITLE', 
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
  inputType: 'mobile',
  label: 'EMPLOYEE.MOBILE',
  name: 'mobile',
  countryCodes: [
    {
      label: '+20',
      value: '+20',
      maxLength: 10,
      regex: /^1[0-2,5]\d{8}$/  
    },
    {
      label: '+966',
      value: '+966',
      maxLength: 9,
      regex: /^5\d{8}$/       
    },
    {
      label: '+962',
      value: '+962',
      maxLength: 9,
      regex: /^7\d{8}$/      
    }
  ]
}
,
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
    label: 'EMPLOYEE.PROFILE_PIC',
    multiple: true,
    accept: 'image/*,.pdf',
    maxFiles: 5
  },
   {
        type: 'select',
        label: 'EMPLOYEE.FUNCTION_CODE',
        name: 'functionCode',
        selectType: 'simple',
        options: [],
        validations: { required: true },
      },
  ];

  constructor(
    private fb: FormBuilder,
    private employeeService: EmployeeService,
    private branchService: BranchService,
   // private roleService: RoleService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private deptService: DepartmentService,
    private jobService: JobService
  ) { }

  ngOnInit() {
    this.initForm();
    this.listenToMobileCodeChange();
    this.loadBranches();
    this.loadDepartments();
    this.loadJobs();
    this.loadFunctionCodes();
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

    branchId: [null, [Validators.required]],
    jobId: [null, [Validators.required]],

    departmentId: [null, [Validators.required]],

    title: ['', [
      Validators.required,
      Validators.minLength(3),
      Validators.maxLength(50)
    ]],

    nationality: ['', [Validators.required,Validators.maxLength(200)]],

    identityNumber: ['', [
      Validators.required,
      Validators.maxLength(100),
      Validators.pattern('^[0-9]+$')
    ]],

    mobile: ['', [Validators.required, Validators.pattern(/^\d+$/)]],
    mobileCode: [null, Validators.required],

    address: ['', Validators.maxLength(500)],

    qualification: ['', [Validators.required,Validators.maxLength(200)]],

    email: ['', [
      Validators.required,
      Validators.email,
      Validators.maxLength(200),
       Validators.email
    ]],

    password: [''],
    confirmPassword: [''],
    attachments: [null],
    functionCode: [null,[Validators.required]],
    

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

       let mobileCode = '+20'; 
    let mobileNumber = emp.mobile || '';

    if (mobileNumber.startsWith('+966')) {
      mobileCode = '+966';
      mobileNumber = mobileNumber.slice(4); 
    } else if (mobileNumber.startsWith('+20')) {
      mobileCode = '+20';
      mobileNumber = mobileNumber.slice(3);
    } else if (mobileNumber.startsWith('+962')) {
      mobileCode = '+962';
      mobileNumber = mobileNumber.slice(4);
    }
      this.formGroup.patchValue({
        fullName: emp.fullName,
        branchId: emp.branchId,
        jobId: emp.jobId,
        departmentId: emp.departmentId,
        title: emp.title,
        nationality: emp.nationality,
        identityNumber: emp.identityNumber,
        mobile: mobileNumber,
        mobileCode: mobileCode,
        address: emp.address,
        qualification: emp.qualification,
        roleIds: emp.roleIds || [],
        email: emp.email,
        functionCode:emp.functionCode,
        

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
    sortDirection: 'DESC'
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
loadDepartments() {
  const req = {
    searchKey: '',
    pageIndex: 1,
    pageSize: 500,
    sortColumn: 'Id',
    sortDirection: 'DESC'
  };

  this.deptService.getAll(req).subscribe(res => {
    const list: { id: number; name: string }[] = res.data.data;

    const options = list.map(b => ({
      label: b.name,
      value: b.id
    }));

    const field = this.formConfig.find(x => x.name === 'departmentId');
    if (field) {
      field.options = options;
    }

    if (this.isEdit && this.employeeId) {
      const departmentId = this.formGroup.get('departmentId')?.value?.toString();

      const selected = options.find(o => o.value === departmentId);
      if (selected) {
        this.formGroup.get('departmentId')?.setValue(selected.value);
      }
    }
  });
}
loadJobs() {
  const req = {
    searchKey: '',
    pageIndex: 1,
    pageSize: 500,
    sortColumn: 'Id',
    sortDirection: 'DESC'
  };

  this.jobService.getAll(req).subscribe(res => {
    const list: { id: number; title: string }[] = res.data.data;

    const options = list.map(b => ({
      label: b.title,
      value: b.id
    }));

    const field = this.formConfig.find(x => x.name === 'jobId');
    console.log('Job field:', field);
    if (field) {
      field.options = options;
    }

    if (this.isEdit && this.employeeId) {
      const jobId = this.formGroup.get('jobId')?.value?.toString();
    console.log('Job:', jobId);
    console.log('form:', this.formGroup.value);

      const selected = options.find(o => o.value === jobId);
      if (selected) {
        this.formGroup.get('jobId')?.setValue(selected.value);
      }
    }
  });
}

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

   if (formValue.mobileCode && formValue.mobile) {
    formData.set('mobile', formValue.mobileCode + formValue.mobile);
  }

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
listenToMobileCodeChange() {
  const mobileControl = this.formGroup.get('mobile');
  const mobileCodeControl = this.formGroup.get('mobileCode');

  mobileCodeControl?.valueChanges.subscribe(code => {

    const mobileField = this.formConfig.find(f => f.name === 'mobile');
    const country = mobileField?.countryCodes?.find(c => c.value === code);

    if (!country) return;

    mobileControl?.setValidators([
      Validators.required,
      Validators.pattern(country.regex),
      Validators.minLength(country.maxLength),
      Validators.maxLength(country.maxLength)
    ]);

    mobileControl?.updateValueAndValidity();
  });
}


loadFunctionCodes() {
  this.employeeService.getFunctionCodes().subscribe({
    next: (res) => {
      this.functionCodes = res.data ?? [];

      const options = this.functionCodes.map(x => ({
        label: x.name,
        value: x.id
      }));

      const field = this.formConfig.find(f => f.name === 'functionCode');
      if (field) field.options = options;

      this.formConfig = [...this.formConfig];
    }
  });
}


}