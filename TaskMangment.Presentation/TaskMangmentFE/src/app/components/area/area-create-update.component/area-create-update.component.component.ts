import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core'; // ← أضف OnInit
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { AreaService } from 'app/core/services/area.service';
import { Router } from '@angular/router'; // ← أضف Router
import { TranslateModule } from '@ngx-translate/core';
import { EmployeeService } from '../../../core/services/employee.service';
import { Employee } from 'app/core/models/employee/employee';

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

  formGroup!: FormGroup; // ← أضف !

  formConfig = [
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
      validations: { maxlength: 200 }, 
      defaultValue: '' 
    },
    {
  type: 'select',
  label: 'AREA.MANAGER',
  name: 'managerId',
  options: [],   // ← سيتم ملؤها لاحقاً
  validations: { required: true },
  defaultValue: null
}

  ];

  constructor(
    private fb: FormBuilder,
    private areaService: AreaService,
    private router: Router,
   private employeeService: EmployeeService
  ) {}

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
      address: ['', Validators.maxLength(200)],
      managerName: ['', Validators.maxLength(100)],
          managerId: [null, Validators.required] 

    });
  }

  loadArea() {
    if (!this.areaId) return;

    this.areaService.getById(this.areaId).subscribe(res => {
      this.formGroup.patchValue({
    name: res.name,
    address: res.address,
    managerId: res.managerID 
  });
    });
  }

 loadEmployees() {
  const request = {
    searchKey: '',
    pageIndex: 1,
    pageSize: 1000,
    sortColumn: 'Id',
    sortDirection: 'ASC'
  };

  this.employeeService.getAll(request).subscribe(res => {
    this.employees = res.data; 
  });
}

  onSubmit(formData: any) {
    console.log('Form submitted with data:', formData);
    
    if (this.formGroup.invalid) {
      console.log('Form is invalid');
      this.formGroup.markAllAsTouched();
      return;
    }

    if (this.isEdit && this.areaId) {
      console.log('Updating area with ID:', this.areaId);
      this.areaService.update(this.areaId, this.formGroup.value).subscribe({
        next: (response) => {
          console.log('Update successful:', response);
          this.formSubmitted.emit();
          //this.router.navigate(['/areas/area-list']); 
        },
        error: (error) => {
          console.error('Update error:', error);
        }
      });
    } else {
      console.log('Creating new area');
      this.areaService.create(this.formGroup.value).subscribe({
        next: (response) => {
          console.log('Create successful:', response);
          this.formSubmitted.emit();
         // this.router.navigate(['/areas/area-list']); 
        },
        error: (error) => {
          console.error('Create error:', error);
          alert(error);
        }
      });
    }
  }
}