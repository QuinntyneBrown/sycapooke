import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AccountsApi, AuthState, shortRef } from '@sycapooke/api';
import {
  AlertComponent,
  ButtonDirective,
  EmptyStateComponent,
  ErrorSummaryComponent,
  FormFieldComponent,
  IconComponent,
} from '@sycapooke/components';

const STORY_MAX = 500;

/**
 * The founder's /shop/story (L2-003, mocks pages/edit-story): the display name, story (up to 500
 * characters) and an optional public city and region. There is no street address.
 */
@Component({
  selector: 'sy-story-page',
  imports: [
    ReactiveFormsModule,
    RouterLink,
    AlertComponent,
    ButtonDirective,
    EmptyStateComponent,
    ErrorSummaryComponent,
    FormFieldComponent,
    IconComponent,
  ],
  templateUrl: './story-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StoryPage {
  private readonly api = inject(AccountsApi);
  private readonly auth = inject(AuthState);

  readonly max = STORY_MAX;
  readonly form = inject(NonNullableFormBuilder).group({ displayName: '', story: '', city: '', region: '' });
  readonly view = signal<'loading' | 'form' | 'saving' | 'saved' | 'error'>('loading');
  readonly errors = signal<Record<string, string>>({});
  readonly failure = signal('');
  readonly count = signal(0);
  readonly initials = signal('');
  readonly publicPath = computed(() => ['/members', this.auth.member()?.displayName ?? '']);
  readonly summary = computed(() =>
    Object.entries(this.errors()).map(([field, message]) => ({
      fieldId: field === 'displayName' ? 'display-name' : field,
      message,
    })),
  );

  constructor() {
    void this.load();
  }

  async load(): Promise<void> {
    this.view.set('loading');
    const result = await this.api.founderStory();
    if (!result.ok) {
      this.failure.set(shortRef(result.problem.correlationId));
      this.view.set('error');
      return;
    }
    this.form.setValue(result.value);
    this.count.set(result.value.story.length);
    this.initials.set(initialsOf(result.value.displayName));
    this.view.set('form');
  }

  typed(): void {
    this.count.set(this.form.getRawValue().story.length);
  }

  async save(): Promise<void> {
    if (this.view() === 'saving') return;
    this.errors.set({});
    const value = this.form.getRawValue();
    if (value.story.length > STORY_MAX) {
      this.errors.set({ story: `Story must be ${STORY_MAX} characters or fewer` });
      return;
    }
    this.view.set('saving');
    const result = await this.api.saveFounderStory(value);
    if (result.ok) {
      await this.auth.reload();
      this.initials.set(initialsOf(result.value.displayName));
      this.view.set('saved');
      return;
    }
    this.view.set('form');
    const fieldErrors = Object.entries(result.problem.errors ?? {});
    if (result.problem.status === 422 && fieldErrors.length > 0) {
      this.errors.set(Object.fromEntries(fieldErrors.map(([field, e]) => [field, e.message])));
      return;
    }
    this.failure.set(shortRef(result.problem.correlationId));
    this.view.set('error');
  }
}

function initialsOf(name: string): string {
  return name
    .split(/[\s_-]+/)
    .filter(Boolean)
    .slice(0, 2)
    .map((part) => part[0].toUpperCase())
    .join('');
}
