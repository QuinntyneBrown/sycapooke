import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AccountsApi, shortRef } from '@sycapooke/api';
import { AlertComponent, ButtonDirective, FormFieldComponent, IconComponent } from '@sycapooke/components';

const EMAIL_PATTERN = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;

/** Asks for a reset link at /forgot-password (L2-009, mocks pages/forgot-password). */
@Component({
  selector: 'sy-forgot-password-page',
  imports: [ReactiveFormsModule, RouterLink, AlertComponent, ButtonDirective, FormFieldComponent, IconComponent],
  templateUrl: './forgot-password-page.html',
  styleUrl: './forgot-password-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForgotPasswordPage {
  private readonly api = inject(AccountsApi);

  readonly form = inject(NonNullableFormBuilder).group({ email: '' });
  readonly state = signal<'form' | 'submitting' | 'sent'>('form');
  readonly emailError = signal<string | null>(null);
  readonly failure = signal<string | null>(null);

  async submit(): Promise<void> {
    if (this.state() === 'submitting') return;
    const email = this.form.getRawValue().email.trim();
    this.failure.set(null);
    if (!EMAIL_PATTERN.test(email)) {
      this.emailError.set('Enter an email like name@example.com');
      return;
    }
    this.emailError.set(null);
    this.state.set('submitting');
    const result = await this.api.requestPasswordReset(email);
    if (result.ok) {
      this.state.set('sent');
      return;
    }
    this.failure.set(shortRef(result.problem.correlationId));
    this.state.set('form');
  }
}
