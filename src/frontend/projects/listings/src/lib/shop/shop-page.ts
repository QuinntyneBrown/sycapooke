import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthState, ListingsApi, shortRef } from '@sycapooke/api';
import { ButtonDirective, EmptyStateComponent, IconComponent, StatComponent } from '@sycapooke/components';

/**
 * The founder's shop overview at /shop (mocks pages/shop): a linked figure for the pieces for sale and
 * links to inventory. Sales, support and story join as their pages are built. Guarded by founderGuard.
 */
@Component({
  selector: 'sy-shop-page',
  imports: [RouterLink, ButtonDirective, EmptyStateComponent, IconComponent, StatComponent],
  templateUrl: './shop-page.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShopPage {
  private readonly listings = inject(ListingsApi);
  readonly auth = inject(AuthState);

  readonly view = signal<'loading' | 'ready' | 'empty' | 'error'>('loading');
  readonly active = signal(0);
  readonly errorRef = signal('');

  constructor() {
    void this.load();
  }

  async load(): Promise<void> {
    this.view.set('loading');
    const result = await this.listings.mine();
    if (!result.ok) {
      this.errorRef.set(shortRef(result.problem.correlationId));
      this.view.set('error');
      return;
    }
    this.active.set(result.value.counts.active);
    this.view.set(result.value.counts.all === 0 ? 'empty' : 'ready');
  }
}
