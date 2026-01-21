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
import { NotificationApiService } from 'app/core/services/notification.service';
import { SignalRService } from 'app/core/services/signalr.service';

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
    public authservice: AuthService,
    private router: Router,
    private formBuilder: FormBuilder,
    private renderer: Renderer2,
    private toastr: ToastrService, private translate: TranslateService, private notificationService: NotificationApiService, private signalRService: SignalRService) {
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

  if (this.loginForm.invalid) {
    this.toastr.error('Please fill in the form correctly', '', {
      timeOut: 3000,
      positionClass: 'toast-top-right',
    });
    return;
  }

  const username = this.loginForm.controls['username'].value;
  const password = this.loginForm.controls['password'].value;

  this.authservice.login(username, password).subscribe({
    next: (response) => {

      const userId = response.data?.userId || 0;

      // Start SignalR
      this.signalRService.startConnection(userId);

      // Load notifications
      this.notificationService.getUnread().subscribe(res => {
        const unread = res.data || [];
        unread.forEach(n => {
         this.toastr.info(n.message, this.translate.instant('nav.notifications.notification'));
        });
      });

      this.router.navigate(['/dashboard']);
    }, 
  });
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