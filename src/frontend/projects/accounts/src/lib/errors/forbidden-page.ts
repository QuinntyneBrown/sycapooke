import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonDirective, CenterStateComponent } from '@sycapooke/components';

/** /403: another customer's page, or a founder-only route opened by a customer (mocks pages/forbidden). */
@Component({
  selector: 'sy-forbidden-page',
  imports: [RouterLink, ButtonDirective, CenterStateComponent],
  template: `
    <sy-center-state code="403" heading="You can’t open this">
      <p class="text-muted">{{ reason() }}</p>
      <a actions syButton="primary" routerLink="/account">Go to your account</a>
    </sy-center-state>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ForbiddenPage {
  /** `management` when a signed-in customer opened a founder-only route. */
  readonly reason = computed(() =>
    this.kind() === 'management'
      ? 'Shop management is only for the shop’s founder.'
      : 'It belongs to another customer’s account.',
  );
  readonly kind = input<string>('', { alias: 'reason' });
}
