import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, OnInit, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { FormFieldConfig } from 'app/core/models/form-field-config';
import { EmployeeService } from 'app/core/services/employee.service';
import { AuthService } from 'app/core/services/auth.service';

@Component({
  selector: 'app-tasks-employee-deep-search',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './tasks-employee-deep-search.component.html'
})
export class TasksEmployeeDeepSearchComponent implements OnInit, OnChanges {

  @Input() statusOptions: { id: number; name: string }[] = [];
  @Input() initial: any;

  @Output() submitFilters = new EventEmitter<any>();
  @Output() cancel = new EventEmitter<void>();

  formGroup!: FormGroup;
  formConfig: FormFieldConfig[] = [];

  priorityOptions = [
    { label: 'TASK.PRIORITY_LOW', value: 1 },
    { label: 'TASK.PRIORITY_MEDIUM', value: 2 },
    { label: 'TASK.PRIORITY_HIGH', value: 3 }
  ];

  constructor(
    private fb: FormBuilder,
    private employeeService: EmployeeService,
    private auth: AuthService

  ) {}

  ngOnInit(): void {
    this.formGroup = this.fb.group({
      direction: [null],
      statusId: [null],

      targetEmployeeId: [null],

      searchKey: [''],
      priorityId: [null],
      createdFrom: [null],
      createdTo: [null],
      dueFrom: [null],
      dueTo: [null],
    });

    this.formConfig = [
      {
        type: 'select',
        label: 'TASK.employee',           
        name: 'targetEmployeeId',
        selectType: 'employee',
        isPaginated: true,

        searchFunction: (searchTerm: string) => {
          const request = {
            searchKey: searchTerm || '',
            pageIndex: 1,
            pageSize: 20,
            sortColumn: 'Id',
            sortDirection: 'DESC'
          };
          return this.employeeService.getAll(request);
        },

        options: [],
      },
      {
        type: 'input',
        label: 'TASK.searchKey',
        name: 'searchKey',
        defaultValue: ''
      },
      {
        type: 'select',
        label: 'TASK.PRIORITY',
        name: 'priorityId',
        selectType: 'simple',
        options: this.priorityOptions,
        defaultValue: null
      },
      {
        type: 'date',
        label: 'TASK.CREATED_FROM',
        name: 'createdFrom',
        defaultValue: null
      },
      {
        type: 'date',
        label: 'TASK.CREATED_TO',
        name: 'createdTo',
        defaultValue: null
      },
      {
        type: 'date',
        label: 'TASK.DUE_FROM',
        name: 'dueFrom',
        defaultValue: null
      },
      {
        type: 'date',
        label: 'TASK.DUE_TO',
        name: 'dueTo',
        defaultValue: null
      }
    ];

    if (this.initial) {
  const cleanInitial = { ...this.initial, targetEmployeeId: null };
  this.formGroup.patchValue(cleanInitial);
}

  }

  ngOnChanges(changes: SimpleChanges): void {
  if (changes['initial'] && this.initial && this.formGroup) {
    const cleanInitial = { ...this.initial, targetEmployeeId: null };
    this.formGroup.patchValue(cleanInitial);
  }
}


  private formatDateOnly(v: any): string | null {
  if (!v) return null;

  const d = v instanceof Date ? v : new Date(v);
  if (isNaN(d.getTime())) return null;

  const y = d.getFullYear();
  const m = String(d.getMonth() + 1).padStart(2, '0');
  const day = String(d.getDate()).padStart(2, '0');
  return `${y}-${m}-${day}`;
}

onSubmit(formData: any) {
  const selected = formData.targetEmployeeId;

  const targetEmployeeId =
    selected == null ? null :
    typeof selected === 'number' ? selected :
    (selected.id ?? selected.value ?? null);

  const filters = {
    direction: formData.direction ?? null,
    statusId: formData.statusId ?? null,

    targetEmployeeId: targetEmployeeId,

    searchKey: (formData.searchKey || '').trim(),
    priorityId: formData.priorityId ?? null,

    createdFrom: this.formatDateOnly(formData.createdFrom),
    createdTo: this.formatDateOnly(formData.createdTo),
    dueFrom: this.formatDateOnly(formData.dueFrom),
    dueTo: this.formatDateOnly(formData.dueTo),
  };

  this.submitFilters.emit(filters);
}


  onSearchClick() {
    this.onSubmit(this.formGroup.getRawValue());
  }

  
    clear() {
  this.formGroup.reset({
    direction: null,
    statusId: null,
    targetEmployeeId: null,
    searchKey: '',
    priorityId: null,
    createdFrom: null,
    createdTo: null,
    dueFrom: null,
    dueTo: null
  });

  this.submitFilters.emit({
    direction: null,
    statusId: null,
    targetEmployeeId: null,
    searchKey: '',
    priorityId: null,
    createdFrom: null,
    createdTo: null,
    dueFrom: null,
    dueTo: null
  });
}

  onCancel() {
    this.cancel.emit();
  }
@Input() set triggerClear(val: number) {
  if (val && this.formGroup) {
    this.clear();
  }
}
}
