import { ChangeDetectionStrategy, Component, computed, inject, signal } from '@angular/core';
import { ShopProblemsApi, ProblemDetail, ProblemOutcome } from '@sycapooke/api';
import {
  AlertComponent,
  ButtonDirective,
  ChoiceComponent,
  DIALOG_DATA,
  DialogComponent,
  DialogRef,
  FormFieldComponent,
  RadioGroupComponent,
} from '@sycapooke/components';

import { formatMoney } from '../money';

/** What Resolve problem needs: the problem as the page shows it. */
export interface ResolveProblemData {
  detail: ProblemDetail;
}

/**
 * "Resolve problem {reference}" (L2-036, mocks dialogs/resolve-problem): refund the customer in full
 * or close without a refund, with an explanation the customer sees. No explanation shows "Add an
 * explanation. The customer sees it." without calling the API. Closes with the resolved problem.
 */
@Component({
  selector: 'sy-resolve-problem-dialog',
  imports: [AlertComponent, ButtonDirective, ChoiceComponent, DialogComponent, FormFieldComponent, RadioGroupComponent],
  templateUrl: './resolve-problem-dialog.html',
  styleUrl: './resolve-problem-dialog.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ResolveProblemDialog {
  private readonly api = inject(ShopProblemsApi);
  readonly ref = inject<DialogRef<ProblemDetail>>(DialogRef);
  readonly data = inject<ResolveProblemData>(DIALOG_DATA);

  readonly outcome = signal<ProblemOutcome>('RefundBuyer');
  readonly reason = signal('');
  readonly state = signal<'default' | 'busy' | 'failed'>('default');
  readonly reasonError = signal<string | null>(null);
  readonly failure = signal('Nothing changed. Check your connection and try again.');

  readonly total = formatMoney(this.data.detail.order.total);
  readonly action = computed(() => 'Send decision');
  readonly busyLabel = computed(() => (this.outcome() === 'RefundBuyer' ? 'Refunding' : 'Closing'));

  async submit(): Promise<void> {
    if (this.state() === 'busy') return;
    const reason = this.reason().trim();
    if (!reason) {
      this.reasonError.set('Add an explanation. The customer sees it.');
      return;
    }
    this.reasonError.set(null);
    this.state.set('busy');
    const result = await this.api.resolve(this.data.detail.problem.reference, this.outcome(), reason);
    if (result.ok) {
      this.ref.close(result.value);
      return;
    }
    const error = result.problem.errors['reason'];
    if (error) {
      this.state.set('default');
      this.reasonError.set(error.message);
      return;
    }
    if (result.problem.status === 409) this.failure.set(result.problem.title);
    this.state.set('failed');
  }
}
