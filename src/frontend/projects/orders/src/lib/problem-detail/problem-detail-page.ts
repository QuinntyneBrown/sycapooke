import { ChangeDetectionStrategy, Component, computed, effect, inject, input, signal, untracked } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ProblemDetail, ShopProblemsApi, shortRef } from '@sycapooke/api';
import {
  AlertComponent,
  ButtonDirective,
  DialogService,
  EmptyStateComponent,
  Fact,
  FactsComponent,
  IconComponent,
  ItemRowComponent,
  StatusTone,
  ThreadComponent,
  ThreadEntry,
} from '@sycapooke/components';

import { formatMoney } from '../money';
import { formatDay } from '../order-dates';
import { problemThread } from '../problem-thread';
import { ResolveProblemData, ResolveProblemDialog } from '../resolve-problem/resolve-problem-dialog';

const STATUS_TONES: Record<string, StatusTone> = {
  Shipped: 'info',
  Delivered: 'info',
  Completed: 'active',
  Refunded: 'removed',
};

/**
 * The founder's problem page at /shop/problems/{reference} in the app shell (L2-036, mocks
 * pages/problem-detail): the reason, the customer's claim with photos and her reply, and a side
 * panel with the order's facts and "Resolve". A resolved problem shows the decision instead.
 * Anyone else gets the load error (the API answers 403).
 */
@Component({
  selector: 'sy-problem-detail-page',
  imports: [
    AlertComponent,
    ButtonDirective,
    EmptyStateComponent,
    FactsComponent,
    IconComponent,
    ItemRowComponent,
    RouterLink,
    ThreadComponent,
  ],
  templateUrl: './problem-detail-page.html',
  styleUrl: './problem-detail-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProblemDetailPage {
  private readonly api = inject(ShopProblemsApi);
  private readonly dialogs = inject(DialogService);

  /** The problem reference, from the route parameter. */
  readonly reference = input.required<string>();

  readonly detail = signal<ProblemDetail | null>(null);
  readonly view = signal<'loading' | 'loaded' | 'error'>('loading');
  readonly errorRef = signal('');

  readonly thread = computed<ThreadEntry[]>(() => {
    const detail = this.detail();
    return detail
      ? problemThread(detail.problem, `${detail.buyerDisplayName} · customer`, `${detail.sellerDisplayName} · you`)
      : [];
  });
  readonly facts = computed<Fact[]>(() => {
    const order = this.detail()?.order;
    if (!order) return [];
    return [
      { label: 'Status', value: order.status, status: STATUS_TONES[order.status] ?? 'info' },
      { label: 'Total', value: formatMoney(order.total), numeric: true },
      { label: 'Delivered', value: order.deliveredAt ? formatDay(order.deliveredAt) : 'Not yet' },
    ];
  });
  /** The decision of a resolved problem (mocks problem-detail/resolved). */
  readonly decision = computed(() => {
    const detail = this.detail();
    const problem = detail?.problem;
    if (!detail || problem?.status !== 'Resolved') return null;
    const when = problem.resolvedAt ? ` ${formatDay(problem.resolvedAt)}.` : '';
    return problem.outcome === 'RefundBuyer'
      ? {
          heading: 'Refunded in full',
          text: `${formatMoney(detail.order.total)} refunded to ${detail.buyerDisplayName}, who has your explanation.${when}`,
        }
      : { heading: 'Closed without a refund', text: `The order is completed.${when}` };
  });
  readonly opened = computed(() => {
    const at = this.detail()?.problem.openedAt;
    return at ? formatDay(at) : '';
  });

  constructor() {
    effect(() => {
      const reference = this.reference();
      untracked(() => void this.load(reference));
    });
  }

  retry(): Promise<void> {
    return this.load(this.reference());
  }

  async resolve(): Promise<void> {
    const detail = this.detail();
    if (!detail) return;
    const ref = this.dialogs.open<ResolveProblemDialog, ResolveProblemData, ProblemDetail>(ResolveProblemDialog, {
      detail,
    });
    const resolved = await ref.closed;
    if (resolved) this.detail.set(resolved);
  }

  private async load(reference: string): Promise<void> {
    this.view.set('loading');
    const result = await this.api.getProblem(reference);
    if (!result.ok) {
      this.errorRef.set(shortRef(result.problem.correlationId));
      this.view.set('error');
      return;
    }
    this.detail.set(result.value);
    this.view.set('loaded');
  }
}
