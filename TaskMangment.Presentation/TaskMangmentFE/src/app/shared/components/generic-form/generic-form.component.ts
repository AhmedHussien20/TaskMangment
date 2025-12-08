import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Location } from '@angular/common'; // Import Location

@Component({
  selector: 'app-generic-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './generic-form.component.html',
  styleUrls: ['./generic-form.component.scss']
})
export class GenericFormComponent implements OnInit {
  @Input() title: string = '';
  @Input() breadcrumbs: string[] = [];
  @Input() activeitem: string = '';
  @Input() formConfig: any[] = [];
  @Output() formSubmit = new EventEmitter<any>(); // Renamed to formSubmit
  form!: FormGroup;
  @Input() formGroup!: FormGroup;
  @Input() generatePassword!: () => string; // Accept the method as input

  constructor(private fb: FormBuilder, private location: Location) { }

  ngOnInit() {
    // this.buildForm();
  }

  // buildForm() {
  //   let formControls: any = {};

  //   this.formConfig.forEach(field => {
  //     const validations = [];
  //     if (field.validations?.required) {
  //       validations.push(Validators.required);
  //     }
  //     if (field.validations?.minlength) {
  //       validations.push(Validators.minLength(field.validations.minlength));
  //     }
  //     if (field.validations?.maxlength) {
  //       validations.push(Validators.maxLength(field.validations.maxlength));
  //     }

  //     // Set field.disabled to false if it is not defined
  //     if (field.disabled === undefined) {
  //       field.disabled = false;
  //     }
      
  //     formControls[field.name] = [field.defaultValue || '', validations];
  //   });

  //   this.form = this.fb.group(formControls);
  // }

  onSubmit() {
    console.log('GenericFormComponent onSubmit triggered'); // Debugging: Log when onSubmit is called
    if (this.formGroup.valid) {
      console.log('Form is valid, emitting formSubmit:', this.formGroup.value); // Debugging: Log form values
      this.formSubmit.emit(this.formGroup.value); // Emit formSubmit
    } else {
      console.warn('Form is invalid, marking all as touched'); // Debugging: Log invalid form
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
    this.location.back(); // Navigate to the previous page
  }
}
