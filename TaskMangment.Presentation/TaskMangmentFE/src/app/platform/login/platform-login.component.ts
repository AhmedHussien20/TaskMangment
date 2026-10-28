import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { PlatformApiService } from '../platform-api.service';
import { PlatformSessionService } from '../platform-session.service';

@Component({
  selector: 'app-platform-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterModule, TranslateModule],
  templateUrl: './platform-login.component.html',
  styleUrls: ['../platform.component.scss']
})
export class PlatformLoginComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private api = inject(PlatformApiService);
  private session = inject(PlatformSessionService);
  private router = inject(Router);

  loading = false;
  showLoginPassword = false;

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]]
  });

  ngOnInit(): void {
    document.body.classList.add('error-1');
    if (this.session.current) {
      this.router.navigateByUrl(this.session.homePath);
    }
  }

  ngOnDestroy(): void {
    document.body.classList.remove('error-1');
  }

  login(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    this.loading = true;
    const value = this.loginForm.getRawValue();
    this.api.login(value.email || '', value.password || '').subscribe({
      next: (res) => {
        this.loading = false;
        this.session.save(res.data);
        this.router.navigateByUrl(this.session.homePath);
      },
      error: () => {
        this.loading = false;
      }
    });
  }
}
