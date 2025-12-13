import { Component, EventEmitter, Input, Output, OnInit } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { NgSelectModule } from '@ng-select/ng-select';

@Component({
  selector: 'app-generic-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule, NgSelectModule],
  templateUrl: './generic-form.component.html',
  styleUrls: ['./generic-form.component.scss']
})
export class GenericFormComponent implements OnInit {

  @Input() title: string = '';
  @Input() breadcrumbs: string[] = [];
  @Input() activeitem: string = '';
  @Input() formConfig: any[] = [];

  @Input() formGroup!: FormGroup;

  @Output() formSubmit = new EventEmitter<any>();

  constructor(private location: Location) { }

  ngOnInit() { }

  onSubmit() {
    console.log('GenericFormComponent onSubmit triggered');

    if (this.formGroup.valid) {
      console.log('Form is valid, emitting:', this.formGroup.value);
      this.formSubmit.emit(this.formGroup.value);
    } else {
      console.warn('Form invalid → marking all touched');
      this.formGroup.markAllAsTouched();
    }
  }

  onGeneratePassword(fieldName: string): void {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()';
    const passwordLength = 12;

    const password = Array(passwordLength)
      .fill(chars)
      .map(x => x[Math.floor(Math.random() * x.length)])
      .join('');

    this.formGroup.get(fieldName)?.setValue(password);
  }

  goBack() {
    this.location.back();
  }

  onImageError(event: any) {
    event.target.src = 'assets/images/user.png';
  }

}
