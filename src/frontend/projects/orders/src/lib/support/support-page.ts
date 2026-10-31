import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ShopProblemsApi, SupportQueue, SupportRequest, shortRef } from '@sycapooke/api';
import {
  ButtonDirective,
  EmptyStateComponent,
  IconComponent,
  ItemRowComponent,
  StatusComponent,
  StatusTone,
} from '@sycapooke/components';

import { formatMoney } from '../money';
import { formatDay } from '../order-dates';

const TONES: Record<SupportRequest['status'], StatusTone> = {
  Open: 'reserved',
  Refunded: 'removed',
  Closed: 'active',
};

/**
 * The founder's support queue at /shop/support (mocks pages/support): customers' problems and
 * cancellation requests, newest first, with an All and an Open filter. Each row links to the page
 * that handles it.
 */
@Component({
  selector: 'sy-support-page',
  imports: [RouterLink, ButtonDirective, EmptyStateComponent, IconComponent, ItemRowComponent, StatusComponent],
  templateUrl: './support-page.html',
  styleUrl: './support-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SupportPage {
  private readonly api = inject(ShopProblemsApi);

  readonly queue = signal<SupportQueue | null>(null);
  readonly view = signal<'loading' | 'loaded' | 'error'>('loading');
  readonly errorRef = signal('');
  readonly filter = signal<'all' | 'open'>('all');

  readonly rows = computed(() => {
    const items = this.queue()?.items ?? [];
    return this.filter() === 'open' ? items.filter((i) => i.status === 'Open') : items;
  });
  readonly subtitle = computed(() => `${this.queue()?.openCount ?? 0} open`);

  readonly money = formatMoney;
  readonly tone = (status: SupportRequest['status']): StatusTone => TONES[status];
  readonly meta = (row: SupportRequest): string =>
    `${row.reference} · ${row.label} · ${row.customer} · ${row.status === 'Open' ? 'opened' : 'resolved'} ${formatDay(row.at)}`;

  constructor() {
    void this.load();
  }

  retry(): Promise<void> {
    return this.load();
  }

  private async load(): Promise<void> {
    this.view.set('loading');
    const result = await this.api.support();
    if (!result.ok) {
      this.errorRef.set(shortRef(result.problem.correlationId));
      this.view.set('error');
      return;
    }
    this.queue.set(result.value);
    this.view.set('loaded');
  }
}
