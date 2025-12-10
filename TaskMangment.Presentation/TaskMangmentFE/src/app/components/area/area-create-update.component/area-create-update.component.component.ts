import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { GenericFormComponent } from 'app/shared/components/generic-form/generic-form.component';
import { AreaService } from 'app/core/services/area.service';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-area-create-update',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TranslateModule,
    GenericFormComponent
  ],
  templateUrl: './area-create-update.component.html'
})
export class AreaCreateUpdateComponent {

  @Input() isEdit: boolean = false;
  @Input() areaId: number | null = null;
  @Output() formSubmitted = new EventEmitter<void>();

  title = 'AREA.title';
  breadcrumbs = ['Home', 'Areas'];
  activeitem = 'AREA.create';

  formGroup: FormGroup;

  formConfig = [
    { type: 'input', label: 'AREA.NAME', name: 'name', validations: { required: true, minlength: 3, maxlength: 100 }, defaultValue: '' },
    { type: 'input', label: 'AREA.ADDRESS', name: 'address', validations: { maxlength: 200 }, defaultValue: '' },
    { type: 'input', label: 'AREA.MANAGER', name: 'managerName', validations: { maxlength: 100 }, defaultValue: '' }
  ];

  constructor(
    private fb: FormBuilder,
    private areaService: AreaService
  ) {
    this.formGroup = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(100)]],
      address: [''],
      managerName: ['']
    });
  }

  loadArea() {
    if (!this.areaId) return;

    this.areaService.getById(this.areaId).subscribe(res => {
      this.formGroup.patchValue(res);
    });
  }

  onSubmit() {
    if (this.formGroup.invalid) return;

    if (this.isEdit && this.areaId) {
      this.areaService.update(this.areaId, this.formGroup.value).subscribe(() => {
        this.formSubmitted.emit();
      });
    } else {
      this.areaService.create(this.formGroup.value).subscribe(() => {
        this.formSubmitted.emit();
      });
    }
  }
}

