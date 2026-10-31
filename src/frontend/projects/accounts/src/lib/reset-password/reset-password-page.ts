import { ChangeDetectionStrategy, Component, inject, input, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AccountsApi, shortRef } from '@sycapooke/api';
import { AlertComponent, ButtonDirective, FormFieldComponent } from '@sycapooke/components';

/** Chooses a new password from an emailed link at /reset-password?token=… (L2-009, mocks pages/reset-password). */
@Component({
  selector: 'sy-reset-password-page',
  imports: [ReactiveFormsModule, RouterLink, AlertComponent, ButtonDirective, FormFieldComponent],
  templateUrl: './reset-password-page.html',
  styleUrl: './reset-password-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResetPasswordPage {
  private readonly api = inject(AccountsApi);

  /** The single-use token of the link, bound from the query string. */
  readonly token = input<string>('');

  readonly form = inject(NonNullableFormBuilder).group({ password: '', confirm: '' });
  readonly state = signal<'form' | 'submitting' | 'saved' | 'expired'>('form');
  readonly passwordError = signal<string | null>(null);
  readonly confirmError = signal<string | null>(null);
  readonly failure = signal<string | null>(null);

  async submit(): Promise<void> {
    if (this.state() === 'submitting') return;
    const { password, confirm } = this.form.getRawValue();
    this.failure.set(null);
    this.passwordError.set(password.length < 12 ? 'Password must be at least 12 characters' : null);
    this.confirmError.set(password.length >= 12 && confirm !== password ? 'The passwords don’t match' : null);
    if (this.passwordError() || this.confirmError()) return;

    this.state.set('submitting');
    const result = await this.api.confirmPasswordReset(this.token() ?? '', password);
    if (result.ok) {
      this.state.set('saved');
      return;
    }
    this.state.set('form');
    if (result.problem.code === 'RESET_LINK_EXPIRED') {
      this.state.set('expired');
    } else if (result.problem.status === 422 && result.problem.errors['password']) {
      this.passwordError.set(result.problem.errors['password'].message);
    } else {
      this.failure.set(shortRef(result.problem.correlationId));
    }
  }
}
