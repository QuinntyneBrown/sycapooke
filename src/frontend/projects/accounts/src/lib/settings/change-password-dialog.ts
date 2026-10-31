import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { AccountsApi, shortRef } from '@sycapooke/api';
import { AlertComponent, ButtonDirective, DialogComponent, DialogRef, FormFieldComponent } from '@sycapooke/components';

/**
 * "Change password" (L2-008, mocks dialogs/change-password): the current and a new password. A wrong
 * current password or a weak new one shows against its field; a failure keeps both and offers
 * "Try again". Closes with true once saved; the server signs the other devices out.
 */
@Component({
  selector: 'sy-change-password-dialog',
  imports: [ReactiveFormsModule, AlertComponent, ButtonDirective, DialogComponent, FormFieldComponent],
  templateUrl: './change-password-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ChangePasswordDialog {
  private readonly api = inject(AccountsApi);
  readonly ref = inject<DialogRef<boolean>>(DialogRef);

  readonly form = inject(NonNullableFormBuilder).group({ current: '', next: '' });
  readonly state = signal<'form' | 'busy' | 'failed'>('form');
  readonly currentError = signal<string | null>(null);
  readonly nextError = signal<string | null>(null);
  readonly failure = signal('');

  async submit(): Promise<void> {
    if (this.state() === 'busy') return;
    const { current, next } = this.form.getRawValue();
    this.currentError.set(current ? null : 'Enter your current password');
    this.nextError.set(next.length >= 12 ? null : 'Password must be at least 12 characters');
    if (this.currentError() || this.nextError()) return;

    this.state.set('busy');
    const result = await this.api.changePassword(current, next);
    if (result.ok) {
      this.ref.close(true);
      return;
    }
    this.state.set('form');
    const errors = result.problem.errors ?? {};
    if (result.problem.status === 422 && (errors['currentPassword'] || errors['newPassword'])) {
      this.currentError.set(errors['currentPassword']?.message ?? null);
      this.nextError.set(errors['newPassword']?.message ?? null);
      return;
    }
    this.failure.set(shortRef(result.problem.correlationId));
    this.state.set('failed');
  }
}
