import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { AuthState } from '@sycapooke/api';
import { AlertComponent, ButtonDirective, DialogService } from '@sycapooke/components';

import { ChangePasswordDialog } from './change-password-dialog';
import { CloseAccountDialog } from './close-account-dialog';
import { SignOutEverywhereDialog } from './sign-out-everywhere-dialog';

/** /account/settings (L2-008, mocks pages/settings): email, password and sessions. */
@Component({
  selector: 'sy-settings-page',
  imports: [AlertComponent, ButtonDirective],
  templateUrl: './settings-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SettingsPage {
  private readonly dialogs = inject(DialogService);
  private readonly router = inject(Router);
  readonly auth = inject(AuthState);

  readonly passwordChanged = signal(false);

  async changePassword(): Promise<void> {
    const done = await this.dialogs.open<ChangePasswordDialog, undefined, boolean>(ChangePasswordDialog).closed;
    this.passwordChanged.set(done === true);
  }

  async closeAccount(): Promise<void> {
    const done = await this.dialogs.open<CloseAccountDialog, undefined, boolean>(CloseAccountDialog).closed;
    if (done === true) {
      this.auth.clear();
      await this.router.navigate(['/']);
    }
  }

  async signOutEverywhere(): Promise<void> {
    const done = await this.dialogs.open<SignOutEverywhereDialog, undefined, boolean>(SignOutEverywhereDialog).closed;
    if (done === true) {
      this.auth.clear();
      await this.router.navigate(['/sign-in']);
    }
  }
}
