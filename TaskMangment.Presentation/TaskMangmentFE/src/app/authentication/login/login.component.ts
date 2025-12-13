import { Component, ElementRef, Inject, NgZone, Renderer2 } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { ToastrModule, ToastrService } from 'ngx-toastr';
import { NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { CommonModule, DOCUMENT } from '@angular/common';
import { DomSanitizer } from '@angular/platform-browser';
import { AuthService } from 'app/core/services/auth.service';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    NgbModule,
    FormsModule,
    ReactiveFormsModule,
    ToastrModule,
    TranslateModule
  ],
  providers: [{ provide: ToastrService, useClass: ToastrService }],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  public showPassword = false;
  disabled = '';

  constructor(
    @Inject(DOCUMENT) private document: Document,
    private elementRef: ElementRef,
    private sanitizer: DomSanitizer,
    public authservice: AuthService,
    private router: Router,
    private formBuilder: FormBuilder,
    private renderer: Renderer2,
    private toastr: ToastrService,private translate: TranslateService) {
  this.translate.use('ar');
  document.documentElement.dir = 'rtl';
  document.documentElement.lang = 'ar';
  }

  ngOnInit(): void {
    this.renderer.addClass(this.document.body, 'error-1');
    this.loginForm = this.formBuilder.group({
      username: ['', [Validators.required]],  // Change userCode to username
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  public loginForm!: FormGroup;
  public error: any = '';

  get form() {
    return this.loginForm.controls;
  }

  getValidationMessages(controlName: string): string[] {
    const control = this.loginForm.controls[controlName];
    const messages: string[] = [];
  
    if (control && control.errors && control.touched) {
      for (const errorKey in control.errors) {
        if (control.errors.hasOwnProperty(errorKey)) {
          messages.push(this.getErrorMessage(controlName, errorKey, control.errors[errorKey]));
        }
      }
    }
    return messages;
  }
  
  getErrorMessage(controlName: string, errorKey: string, errorValue: any): string {
    const fieldNames: { [key: string]: string } = {
      username: 'Username',  // Change userCode to username
      password: 'Password',
      email: 'Email'
    };
  
    const defaultFieldName = fieldNames[controlName] || controlName;
  
    const errorMessages: { [key: string]: string } = {
      required: `${defaultFieldName} is required.`,
      minlength: `${defaultFieldName} must be at least ${errorValue.requiredLength} characters.`,
      maxlength: `${defaultFieldName} must not exceed ${errorValue.requiredLength} characters.`,
      pattern: `${defaultFieldName} format is invalid.`,
      email: `Please enter a valid ${defaultFieldName}.`
    };
  
    return errorMessages[errorKey] || `${defaultFieldName} is invalid.`;
  }
  

  Submit(event: Event) {
    event.preventDefault();
    console.log(this.loginForm.value);
    if (this.loginForm.valid) {
      const username = this.loginForm.controls['username'].value;  // Change userCode to username
      const password = this.loginForm.controls['password'].value;

      this.authservice.login(username, password).subscribe({  // Change userCode to username
        next: (response) => {
          this.router.navigate(['/dashboard']);
        },
        error: (error) => {
          console.log(error);
          this.toastr.error(error, 'error', {
            timeOut: 3000,
            positionClass: 'toast-top-right',
          });
        }
      });
    } else {
      this.toastr.error('Please fill in the form correctly', 'spruha', {
        timeOut: 3000,
        positionClass: 'toast-top-right',
      });
    }
  }

  public togglePassword() {
    this.showPassword = !this.showPassword;
  }

  ngOnDestroy(): void {
    const bodyElement = this.renderer.selectRootElement('body', true);
    this.renderer.removeAttribute(bodyElement, 'class');
  }

  toggleClass = "off-line";
  toggleVisibility() {
    this.showPassword = !this.showPassword;
    if (this.toggleClass === "off-line") {
      this.toggleClass = "line";
    } else {
      this.toggleClass = "off-line";
    }
  }
}