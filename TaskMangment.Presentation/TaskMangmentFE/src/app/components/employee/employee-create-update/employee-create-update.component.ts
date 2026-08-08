import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { EmployeeService } from 'app/core/services/employee.service';
import { BranchService } from 'app/core/services/branch.service';
import { RoleService } from 'app/core/services/role.service';
import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormFieldConfig } from 'app/core/models/form-field-config';
import { DepartmentService } from 'app/core/services/department.service';
import { EnumItemDto } from 'app/core/models/employee/employee';
import { AuthService } from 'app/core/services/auth.service';
import { Permissions } from 'app/core/constants/permissions';

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
  /** Profile self-edit: lock org/contact fields the employee must not change. */
  @Input() isProfileMode: boolean = false;
  @Output() formSubmitted = new EventEmitter<void>();
  functionCodes: EnumItemDto[] = [];


  branches: any[] = [];
  roles: any[] = [];

  title = 'EMPLOYEE.TITLE';
  breadcrumbs = ['HOME', 'EMPLOYEES'];
  activeitem = 'EMPLOYEE.CREATE';

  /** Original active flag when editing — used if user cannot enable/disable. */
  private originalIsActive = true;
  /** True when opened via /employee/edit/:id or /employee/add (not list modal). */
  private fromRoute = false;
  canEnableEmployee = false;
  canDisableEmployee = false;

  private readonly maxRolesPerEmployee = 2;

  /** Fields locked on profile self-edit. */
  private readonly profileLockedFields = [
    'mobile',
    'mobileCode',
    'email',
    'employeeTypeId',
    'branchId',
    'isActive',
    'title',
    'roleIds'
  ];

  formGroup!: FormGroup;
  formConfig: FormFieldConfig[] = [
    {
      type: 'input',
      label: 'EMPLOYEE.FULL_NAME',
      name: 'fullName',
      defaultValue: ''
    },
    {
      type: 'input',
      inputType: 'email',
      label: 'EMPLOYEE.EMAIL',
      name: 'email',
      defaultValue: ''
    },
    {
      type: 'input',
      inputType: 'mobile',
      label: 'EMPLOYEE.MOBILE',
      name: 'mobile',
      countryCodes: [
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
    {
      type: 'input',
      label: 'EMPLOYEE.JOB_TITLE',
      name: 'title',
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
      label: 'EMPLOYEE.DEPARTMENT',
      selectType: 'simple',
      name: 'departmentId',
      options: []
    },
    {
      type: 'select',
      label: 'EMPLOYEE.ROLES',
      selectType: 'simple',
      name: 'roleIds',
      multiple: true,
      maxSelectedItems: 2,
      options: [],
      isPaginated: true,
      validations: { required: false }
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
      validations: { required: !this.isEdit, maxlength: 500, equalto: 'password' },
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
      name: 'employeeTypeId',
      selectType: 'simple',
      defaultValue: null,
      options: [],
      validations: { required: true },
    },
    {
      type: 'checkbox',
      label: 'EMPLOYEE.IS_ACTIVE',
      name: 'isActive',
      defaultValue: true
    },
  ];

  constructor(
    private fb: FormBuilder,
    private employeeService: EmployeeService,
    private branchService: BranchService,
    private roleService: RoleService,
    private toastr: ToastrService,
    private translate: TranslateService,
    private deptService: DepartmentService,
    private auth: AuthService,
    private route: ActivatedRoute,
    private router: Router
  ) { }

  ngOnInit() {
    this.resolveRouteMode();
    this.canEnableEmployee = this.auth.hasPermission(Permissions.ENABLE_EMPLOYEE);
    this.canDisableEmployee = this.auth.hasPermission(Permissions.DISABLE_EMPLOYEE);
    this.initForm();
    this.applyActiveToggleVisibility();
    this.applyProfileFieldLocks();
    this.formGroup.get('branchId')!.valueChanges.subscribe(branchId => {
      this.loadDepartments(Number(branchId));
    });

    this.listenToMobileCodeChange();
    this.loadBranches();
    this.loadRoles();
    this.loadFunctionCodes();
    if (this.isEdit && this.employeeId) {
      this.loadEmployee();
    }
  }

  /** When opened as /employee/edit/:id or /employee/add — not when embedded in a modal (e.g. 360). */
  private resolveRouteMode(): void {
    const url = this.router.url || '';

    if (url.includes('/employee/edit/')) {
      const idParam = this.route.snapshot.paramMap.get('id');
      const id = Number(idParam);
      if (!Number.isFinite(id) || id <= 0) return;

      this.fromRoute = true;
      this.isEdit = true;
      this.employeeId = id;
      this.title = 'EMPLOYEE.EDIT';
      this.activeitem = 'EMPLOYEE.EDIT';
      this.breadcrumbs = ['MENU.HOME', 'MENU.EMPLOYEES', 'EMPLOYEE.LIST_TITLE', 'EMPLOYEE.EDIT'];
      return;
    }

    if (url.includes('/employee/add') && !this.employeeId) {
      this.fromRoute = true;
      this.isEdit = false;
      this.title = 'EMPLOYEE.ADD';
      this.activeitem = 'EMPLOYEE.CREATE';
      this.breadcrumbs = ['MENU.HOME', 'MENU.EMPLOYEES', 'EMPLOYEE.LIST_TITLE', 'EMPLOYEE.CREATE'];
    }
  }

  private afterSubmitSuccess(): void {
    this.formSubmitted.emit();
    if (!this.fromRoute) return;

    if (this.isEdit && this.employeeId) {
      this.router.navigate(['/employee/360', this.employeeId]);
    } else {
      this.router.navigate(['/employee/employee-list']);
    }
  }

  /** Hide "موظف نشط" unless user has ENABLE_EMPLOYEE or DISABLE_EMPLOYEE. */
  private applyActiveToggleVisibility(): void {
    if (this.isProfileMode) return;
    const canToggle = this.canEnableEmployee || this.canDisableEmployee;
    const idx = this.formConfig.findIndex(f => f.name === 'isActive');
    if (!canToggle && idx >= 0) {
      this.formConfig.splice(idx, 1);
    }
  }

  /** Profile: employee cannot change mobile, email, type, branch, active, job title, roles. */
  private applyProfileFieldLocks(): void {
    if (!this.isProfileMode) return;

    for (const name of this.profileLockedFields) {
      const field = this.formConfig.find(f => f.name === name);
      if (field) field.disabled = true;
      this.formGroup.get(name)?.disable({ emitEvent: false });
    }

    for (const hide of ['isActive', 'roleIds']) {
      const idx = this.formConfig.findIndex(f => f.name === hide);
      if (idx >= 0) this.formConfig.splice(idx, 1);
    }

    this.formConfig = [...this.formConfig];
  }

  initForm() {
    this.formGroup = this.fb.group({
      fullName: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(250)
      ]],

      branchId: [null, [Validators.required]],

      departmentId: [null, [Validators.required]],

      title: ['', [
        Validators.required,
        Validators.minLength(3),
        Validators.maxLength(50)
      ]],

      roleIds: [[]],

      nationality: ['', [Validators.required, Validators.maxLength(200)]],

      identityNumber: ['', [
        Validators.required,
        Validators.maxLength(100),
        Validators.pattern('^[0-9]+$')
      ]],

      mobile: ['', [Validators.required, Validators.pattern(/^\d+$/)]],
      mobileCode: [null, Validators.required],

      address: ['', [Validators.required, Validators.maxLength(500)]],

      qualification: ['', [Validators.required, Validators.maxLength(200)]],

      email: ['', [
        Validators.required,
        Validators.email,
        Validators.maxLength(200)
      ]],

      password: [''],
      confirmPassword: [''],
      attachments: [null],
      employeeTypeId: [null, Validators.required],
      isActive: [true],


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

      let mobileCode = '+966';
      let mobileNumber = emp.mobile || '';

      if (mobileNumber.startsWith('+966')) {
        mobileCode = '+966';
        mobileNumber = mobileNumber.slice(4);
      } else if (mobileNumber.startsWith('+962')) {
        mobileCode = '+962';
        mobileNumber = mobileNumber.slice(4);
      }
      this.formGroup.patchValue({
        fullName: emp.fullName,
        branchId: emp.branchId,
        departmentId: emp.departmentId,
        title: emp.title,
        roleIds: emp.roleIds ?? [],
        nationality: emp.nationality,
        identityNumber: emp.identityNumber,
        mobile: mobileNumber,
        mobileCode: mobileCode,
        address: emp.address,
        qualification: emp.qualification,
        email: emp.email,
        employeeTypeId: emp.employeeTypeId ?? emp.functionCode,
        isActive: emp.isActive ?? true,
        password: '',
        confirmPassword: '',
      }, { emitEvent: false });

      this.originalIsActive = emp.isActive ?? true;
      this.loadDepartments(emp.branchId, emp.departmentId);

      // Prefill selected role labels for edit (may be outside first page).
      const roleField = this.formConfig.find(f => f.name === 'roleIds');
      if (roleField && Array.isArray(emp.roleIds) && emp.roleIds.length) {
        const names: string[] = emp.roles ?? [];
        const selectedOpts = emp.roleIds.map((id: number, i: number) => ({
          value: id,
          label: names[i] || String(id)
        }));
        roleField.options = this.mergeRoleOptions(selectedOpts);
        this.formConfig = [...this.formConfig];
      }

      this.applyProfileFieldLocks();
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
          this.formGroup.get('branchId')?.setValue(selected.value, { emitEvent: false });
        }
      }
    });
  }

  loadRoles() {
    if (this.isProfileMode) return;

    const field = this.formConfig.find(x => x.name === 'roleIds');
    if (!field) return;

    field.isPaginated = true;
    field.searchFunction = (searchTerm: string, page: number) => {
      return this.roleService.getAll({
        searchKey: searchTerm || '',
        pageIndex: page,
        pageSize: 20,
        sortColumn: 'Id',
        sortDirection: 'DESC'
      });
    };

    field.searchFunction('', 1).subscribe(res => {
      const pageOptions = (res?.data?.data ?? []).map((r: { id: number; name: string }) => ({
        label: r.name,
        value: r.id
      }));
      // Keep already-selected roles visible (edit) when they are outside page 1.
      field.options = this.mergeRoleOptions(pageOptions);
      this.formConfig = [...this.formConfig];
    });
  }

  /** Merge incoming role options with currently selected ones (by id). */
  private mergeRoleOptions(incoming: { label: string; value: number }[]): { label: string; value: number }[] {
    const selectedIds: number[] = (this.formGroup.get('roleIds')?.value ?? [])
      .map((x: any) => Number(x))
      .filter((x: number) => Number.isFinite(x) && x > 0);

    const byId = new Map<number, { label: string; value: number }>();
    for (const opt of [...(this.formConfig.find(f => f.name === 'roleIds')?.options ?? []), ...incoming]) {
      const id = Number(opt.value);
      if (Number.isFinite(id)) byId.set(id, { label: opt.label, value: id });
    }

    // Prefer keeping selected entries even if not in incoming page.
    for (const id of selectedIds) {
      if (!byId.has(id)) {
        byId.set(id, { label: String(id), value: id });
      }
    }

    const selected = selectedIds.map(id => byId.get(id)!).filter(Boolean);
    const rest = incoming.filter(o => !selectedIds.includes(Number(o.value)));
    return [...selected, ...rest];
  }

  loadDepartments(branchId: number, selectedDepartmentId?: number) {

    const req = {
      searchKey: '',
      pageIndex: 1,
      pageSize: 300,
      sortColumn: 'Id',
      sortDirection: 'DESC',
      branchId: branchId
    };

    this.deptService.getAll(req).subscribe(res => {

      const list: { id: number; name: string }[] = res.data.data;

      const options = list.map(d => ({
        label: d.name,
        value: d.id
      }));

      const field = this.formConfig.find(x => x.name === 'departmentId');
      if (field) {
        field.options = options;
      }

      if (selectedDepartmentId) {
        this.formGroup.get('departmentId')?.setValue(selectedDepartmentId);
      } else {
        this.formGroup.get('departmentId')?.setValue(null);
      }
    });
  }

  onSubmit(formValue: any) {
    const raw = this.formGroup.getRawValue();
    formValue = { ...formValue, ...raw };

    if (this.formGroup.invalid) {
      this.formGroup.markAllAsTouched();
      this.toastr.error(this.translate.instant('FORM.VALIDATION_ERROR'));
      return;
    }

    const roleIds: number[] = Array.isArray(formValue.roleIds)
      ? formValue.roleIds.map((x: any) => Number(x)).filter((x: number) => Number.isFinite(x) && x > 0)
      : [];

    if (!this.isProfileMode && roleIds.length > this.maxRolesPerEmployee) {
      this.toastr.error(this.translate.instant('ROLE.MAX_ROLES_EXCEEDED'));
      return;
    }

    const hasActiveField = this.formConfig.some(f => f.name === 'isActive');
    const nextIsActive = hasActiveField
      ? !!formValue.isActive
      : (this.isEdit ? this.originalIsActive : true);

    if (this.isEdit) {
      if (nextIsActive !== this.originalIsActive) {
        if (nextIsActive && !this.canEnableEmployee) {
          this.toastr.error(this.translate.instant('COMMON.NOT_ALLOWED') || 'Not Allowed (No Permission)');
          return;
        }
        if (!nextIsActive && !this.canDisableEmployee) {
          this.toastr.error(this.translate.instant('COMMON.NOT_ALLOWED') || 'Not Allowed (No Permission)');
          return;
        }
      }
    } else if (!nextIsActive && !this.canDisableEmployee) {
      this.toastr.error(this.translate.instant('COMMON.NOT_ALLOWED') || 'Not Allowed (No Permission)');
      return;
    }

    const formData = new FormData();

    Object.keys(formValue).forEach(key => {
      if (key === 'jobId' || key === 'roleIds') return;
      if (key !== 'attachments' && key !== 'isActive' && formValue[key] !== null && formValue[key] !== undefined) {
        formData.append(key, formValue[key]);
      }
    });

    formData.append('jobId', '');

    if (!this.isProfileMode) {
      formData.append('UpdateRoles', 'true');
      roleIds.forEach(id => formData.append('RoleIds', String(id)));
    }

    formData.set('isActive', String(nextIsActive));

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
          this.afterSubmitSuccess();
        }
      });
    }

    else {
      this.employeeService.create(formData).subscribe({
        next: () => {
          this.toastr.success(this.translate.instant('EMPLOYEE.CREATE_SUCCESS'));
          this.afterSubmitSuccess();
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

        const field = this.formConfig.find(f => f.name === 'employeeTypeId');
        if (field) field.options = options;

        this.formConfig = [...this.formConfig];
      }
    });
  }


}
