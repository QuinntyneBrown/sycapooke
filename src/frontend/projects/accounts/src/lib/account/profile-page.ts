import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { AccountsApi, AuthState, shortRef } from '@sycapooke/api';
import {
  AlertComponent,
  ButtonDirective,
  EmptyStateComponent,
  FormFieldComponent,
  IconComponent,
} from '@sycapooke/components';

/** /account/profile (L2-004, mocks pages/edit-profile): rename the private display name. */
@Component({
  selector: 'sy-profile-page',
  imports: [
    ReactiveFormsModule,
    AlertComponent,
    ButtonDirective,
    EmptyStateComponent,
    FormFieldComponent,
    IconComponent,
  ],
  templateUrl: './profile-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfilePage {
  private readonly api = inject(AccountsApi);
  private readonly auth = inject(AuthState);

  readonly form = inject(NonNullableFormBuilder).group({ displayName: '', email: '' });
  readonly view = signal<'loading' | 'form' | 'saving' | 'saved' | 'error'>('loading');
  readonly nameError = signal<string | null>(null);
  readonly failure = signal('');

  constructor() {
    void this.load();
  }

  async load(): Promise<void> {
    this.view.set('loading');
    const result = await this.api.privateAccount();
    if (result.ok) {
      this.form.setValue({ displayName: result.value.displayName, email: result.value.email });
      this.view.set('form');
      return;
    }
    this.failure.set(shortRef(result.problem.correlationId));
    this.view.set('error');
  }

  async save(): Promise<void> {
    if (this.view() === 'saving') return;
    this.nameError.set(null);
    this.view.set('saving');
    const result = await this.api.updatePrivateAccount(this.form.getRawValue().displayName);
    if (result.ok) {
      await this.auth.reload();
      this.view.set('saved');
      return;
    }
    this.view.set('form');
    const message = result.problem.errors?.['displayName']?.message;
    if (result.problem.status === 422 && message) {
      this.nameError.set(message);
      return;
    }
    this.failure.set(shortRef(result.problem.correlationId));
    this.view.set('error');
  }
}
