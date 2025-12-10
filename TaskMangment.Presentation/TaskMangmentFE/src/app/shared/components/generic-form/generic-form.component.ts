import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormGroup, ReactiveFormsModule } from '@angular/forms';
import { CommonModule, Location } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-generic-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule,TranslateModule],
  templateUrl: './generic-form.component.html',
  styleUrls: ['./generic-form.component.scss']
})
export class GenericFormComponent implements OnInit {
  @Input() title: string = '';
  @Input() breadcrumbs: string[] = [];
  @Input() activeitem: string = '';
  @Input() formConfig: any[] = [];

  @Input() formGroup!: FormGroup;

  // 👇 خليه اسمه submit علشان تقدر تستخدم (submit)="onSubmit($event)"
  @Output() submit = new EventEmitter<any>();

  constructor(private location: Location) { }

  ngOnInit() {
    // مفيش حاجة هنا دلوقتي لأن الـ form بيتبنى في الـ parent
  }

  onSubmit() {
    console.log('GenericFormComponent onSubmit triggered');
    if (this.formGroup.valid) {
      console.log('Form is valid, emitting submit:', this.formGroup.value);
      this.submit.emit(this.formGroup.value);
    } else {
      console.warn('Form is invalid, marking all as touched');
      this.formGroup.markAllAsTouched();
    }
  }

  onGeneratePassword(fieldName: string): void {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()';
    const passwordLength = 12;
    const password = Array(passwordLength)
      .fill(chars)
      .map(x => x[Math.floor(Math.random() * x.length)]).join('');

    this.formGroup.get(fieldName)?.setValue(password);
  }

  goBack() {
    this.location.back();
  }
}
