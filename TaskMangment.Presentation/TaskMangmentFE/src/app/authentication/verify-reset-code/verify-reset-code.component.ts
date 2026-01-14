import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from 'app/core/services/auth.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-verify-reset-code',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule],
  templateUrl: './verify-reset-code.component.html',
  styleUrls: ['./verify-reset-code.component.scss']
})
export class VerifyResetCodeComponent implements OnInit {
  form!: FormGroup;
  email!: string;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    private toastr: ToastrService
  ) {}

 ngOnInit(): void {
  this.email = localStorage.getItem('resetEmail') || '';

  if (!this.email) {
    this.toastr.error('Email is required');
    this.router.navigate(['/auth/forgot-password']);
  }

  this.form = this.fb.group({
    token: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(6)]]
  });
}


 submit(): void {
  if (this.form.invalid || !this.email) return;

  this.isLoading = true;
  const token = this.form.value.token;

  this.authService.verifyResetCode({
    email: this.email,
    token: token
  }).subscribe({
    next: (response: any) => {
      this.isLoading = false;
      this.toastr.success('Token verified successfully!');

      localStorage.setItem('resetToken', token);

      this.router.navigate(['/auth/reset-password']);
    },
    error: (err) => {
      this.isLoading = false;
      this.toastr.error(err.error?.message || 'Invalid or expired token');
    }
  });
}


  resendCode(): void {
    if (!this.email) return;

    this.isLoading = true;
    this.authService.forgotPassword(this.email).subscribe({
      next: () => {
        this.isLoading = false;
        this.toastr.success('New verification code sent to your email');
      },
      error: (err) => {
        this.isLoading = false;
        this.toastr.error(err.error?.message || 'Failed to send code');
      }
    });
  }
}