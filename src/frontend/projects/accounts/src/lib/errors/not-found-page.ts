import { ChangeDetectionStrategy, Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonDirective, CenterStateComponent } from '@sycapooke/components';

/** The page for any unknown address (mocks pages/not-found). */
@Component({
  selector: 'sy-not-found-page',
  imports: [RouterLink, ButtonDirective, CenterStateComponent],
  template: `
    <sy-center-state code="404" heading="Page not found">
      <p class="text-muted">The link may be old or mistyped.</p>
      <a actions syButton="primary" routerLink="/">Back to shop</a>
    </sy-center-state>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotFoundPage {}
