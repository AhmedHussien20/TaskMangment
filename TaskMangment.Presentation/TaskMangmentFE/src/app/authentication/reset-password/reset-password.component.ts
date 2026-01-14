import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from 'app/core/services/auth.service';

@Component({
  selector: 'app-reset-password',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterModule,
    TranslateModule
  ],
  templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent {
  form!: FormGroup;
  email!: string;
  token!: string;
  isTokenValid: boolean = false;
  loading: boolean = true;
  errorMessage: string = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private toastr: ToastrService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // قراءة الايميل والتوكن من localStorage
    this.email = localStorage.getItem('resetEmail') || '';
    this.token = localStorage.getItem('resetToken') || '';

    if (!this.email || !this.token) {
      this.errorMessage = 'Invalid reset link';
      this.loading = false;
      // الرجوع لصفحة forgot-password
      this.router.navigate(['/auth/forgot-password']);
      return;
    }

    // التحقق من التوكن
    this.authService.verifyResetCode({ email: this.email, token: this.token })
      .subscribe({
        next: () => {
          this.isTokenValid = true;
          this.loading = false;

          this.form = this.fb.group({
            newPassword: ['', [Validators.required, Validators.minLength(6)]],
            confirmPassword: ['', [Validators.required]]
          });
        },
        error: err => {
          this.errorMessage = err.error?.message || 'Invalid or expired token';
          this.loading = false;
        }
      });
  }

  submit(): void {
    if (this.form.invalid || this.form.value.newPassword !== this.form.value.confirmPassword) {
      this.toastr.error('Passwords do not match');
      return;
    }

    this.authService.updatePassword({
      email: this.email,
      newPassword: this.form.value.newPassword
    }).subscribe({
      next: () => {
        this.toastr.success('Password reset successfully');

        localStorage.removeItem('resetEmail');
        localStorage.removeItem('resetToken');

        this.router.navigate(['/auth/login']);
      },
      error: err => {
        this.toastr.error(err.error?.message || 'Reset failed');
      }
    });
  }
}
