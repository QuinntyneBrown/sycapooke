import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AccountsApi, PrivateAccount, shortRef } from '@sycapooke/api';
import { ButtonDirective, EmptyStateComponent, IconComponent } from '@sycapooke/components';

/** /account (L2-004, mocks pages/account): a greeting and links; only the signed-in customer sees it. */
@Component({
  selector: 'sy-account-page',
  imports: [RouterLink, ButtonDirective, EmptyStateComponent, IconComponent],
  templateUrl: './account-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AccountPage {
  private readonly api = inject(AccountsApi);

  readonly view = signal<'loading' | 'ready' | 'error'>('loading');
  readonly account = signal<PrivateAccount | null>(null);
  readonly errorRef = signal('');

  constructor() {
    void this.load();
  }

  async load(): Promise<void> {
    this.view.set('loading');
    const result = await this.api.privateAccount();
    if (result.ok) {
      this.account.set(result.value);
      this.view.set('ready');
      return;
    }
    this.errorRef.set(shortRef(result.problem.correlationId));
    this.view.set('error');
  }

  /** "Mar 2025" from the yyyy-MM-dd day the account was created. */
  since(day: string): string {
    return new Date(`${day}T00:00:00Z`).toLocaleDateString('en-GB', {
      month: 'short',
      year: 'numeric',
      timeZone: 'UTC',
    });
  }
}
