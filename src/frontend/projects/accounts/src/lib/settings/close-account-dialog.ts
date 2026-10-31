import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AccountsApi, shortRef } from '@sycapooke/api';
import { AlertComponent, ButtonDirective, DialogComponent, DialogRef, FormFieldComponent } from '@sycapooke/components';

interface BlockingOrder {
  id: string;
  reference: string;
  status: string;
}

/**
 * "Close account?" (L2-005, mocks dialogs/close-account): a customer confirms with their password.
 * Open orders block it and are listed as links. A Google-only account is asked to sign in with Google
 * again. Closes with true once closed.
 */
@Component({
  selector: 'sy-close-account-dialog',
  imports: [AlertComponent, ButtonDirective, DialogComponent, FormFieldComponent, RouterLink],
  templateUrl: './close-account-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class CloseAccountDialog {
  private readonly api = inject(AccountsApi);
  readonly ref = inject<DialogRef<boolean>>(DialogRef);

  readonly state = signal<'form' | 'busy' | 'failed' | 'blocked' | 'google'>('form');
  readonly password = signal('');
  readonly passwordError = signal<string | null>(null);
  readonly failure = signal('');
  readonly blocking = signal<BlockingOrder[]>([]);

  async submit(): Promise<void> {
    if (this.state() === 'busy') return;
    if (!this.password()) {
      this.passwordError.set('Enter your password to confirm.');
      return;
    }
    this.passwordError.set(null);
    this.state.set('busy');
    const result = await this.api.closeAccount(this.password());
    if (result.ok) {
      this.ref.close(true);
      return;
    }
    const { problem } = result;
    if (problem.code === 'ACCOUNT_HAS_OPEN_ORDERS') {
      this.blocking.set(
        Object.entries(problem.errors).map(([id, e]) => ({ id, reference: e.message, status: e.code })),
      );
      this.state.set('blocked');
      return;
    }
    if (problem.code === 'GOOGLE_PROOF_REQUIRED') {
      this.state.set('google');
      return;
    }
    const message = problem.errors['password']?.message;
    if (problem.status === 422 && message) {
      this.passwordError.set(message);
      this.state.set('form');
      return;
    }
    this.failure.set(shortRef(problem.correlationId));
    this.state.set('failed');
  }
}
