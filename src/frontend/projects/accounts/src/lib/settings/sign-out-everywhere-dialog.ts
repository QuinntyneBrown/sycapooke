import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { AccountsApi, shortRef } from '@sycapooke/api';
import { AlertComponent, ButtonDirective, DialogComponent, DialogRef } from '@sycapooke/components';

/**
 * "Sign out everywhere?" (L2-008, mocks dialogs/sign-out-everywhere): revokes every session, this one
 * included. A failure keeps the dialog open with "Try again". Closes with true once done.
 */
@Component({
  selector: 'sy-sign-out-everywhere-dialog',
  imports: [AlertComponent, ButtonDirective, DialogComponent],
  templateUrl: './sign-out-everywhere-dialog.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SignOutEverywhereDialog {
  private readonly api = inject(AccountsApi);
  readonly ref = inject<DialogRef<boolean>>(DialogRef);

  readonly state = signal<'form' | 'busy' | 'failed'>('form');
  readonly failure = signal('');

  async submit(): Promise<void> {
    if (this.state() === 'busy') return;
    this.state.set('busy');
    const result = await this.api.signOutEverywhere();
    if (result.ok) {
      this.ref.close(true);
      return;
    }
    this.failure.set(shortRef(result.problem.correlationId));
    this.state.set('failed');
  }
}
