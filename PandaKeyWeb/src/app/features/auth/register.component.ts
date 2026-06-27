import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../core/auth/auth.service';
import { LanguageSwitcherComponent } from '../../shared/language-switcher/language-switcher.component';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule, LanguageSwitcherComponent],
  template: `
  <div class="container py-5" style="max-width: 460px;">
    <div class="d-flex justify-content-between align-items-center mb-4">
      <h2 class="text-pk fw-bold m-0">{{ 'app.title' | translate }}</h2>
      <app-language-switcher />
    </div>
    <div class="card shadow-sm">
      <div class="card-body p-4">
        <h4 class="mb-3">{{ 'auth.register' | translate }}</h4>
        <div class="mb-3">
          <label class="form-label">{{ 'auth.fullName' | translate }}</label>
          <input class="form-control" [(ngModel)]="fullName" />
        </div>
        <div class="mb-3">
          <label class="form-label">{{ 'auth.email' | translate }}</label>
          <input class="form-control" type="email" [(ngModel)]="email" />
        </div>
        <div class="mb-3">
          <label class="form-label">{{ 'auth.phone' | translate }}</label>
          <input class="form-control" [(ngModel)]="phone" />
        </div>
        <div class="mb-3">
          <label class="form-label">{{ 'auth.password' | translate }}</label>
          <input class="form-control" type="password" [(ngModel)]="password" />
        </div>
        <div class="alert alert-danger py-2" *ngIf="error()">{{ error()! | translate }}</div>
        <button class="btn btn-pk w-100" [disabled]="loading()" (click)="submit()">
          <span *ngIf="loading()" class="spinner-border spinner-border-sm me-2"></span>
          {{ 'auth.doRegister' | translate }}
        </button>
        <div class="text-center mt-3">
          <a routerLink="/auth/login">{{ 'auth.haveAccount' | translate }}</a>
        </div>
      </div>
    </div>
  </div>
  `
})
export class RegisterComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  fullName = '';
  email = '';
  phone = '';
  password = '';
  loading = signal(false);
  error = signal<string | null>(null);

  submit(): void {
    if (!this.fullName || !this.email || !this.password) { this.error.set('auth.errorRegister'); return; }
    this.loading.set(true);
    this.error.set(null);
    this.auth.register(this.fullName, this.email, this.phone, this.password).subscribe({
      next: (res) => {
        this.loading.set(false);
        this.router.navigate([res.role === 'admin' ? '/admin/users' : '/app/zones']);
      },
      error: () => { this.loading.set(false); this.error.set('auth.errorRegister'); }
    });
  }
}
