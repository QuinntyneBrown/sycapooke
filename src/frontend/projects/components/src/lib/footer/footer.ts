import { ChangeDetectionStrategy, Component, inject, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { APP_ORIGIN, externalHref } from '../app-origin';
import { LogoComponent } from '../logo/logo';

/**
 * App-shell footer on ink (design system: Footer): the light logo and a line about Sycapooke,
 * three link columns (Shop, Sell, Sycapooke; the last with a "Cookie settings" button that emits
 * cookieSettings) and a bottom bar with the copyright year and the tagline.
 */
@Component({
  selector: 'sy-footer',
  imports: [RouterLink, LogoComponent],
  templateUrl: './footer.html',
  styleUrl: './footer.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FooterComponent {
  readonly cookieSettings = output();
  readonly year = new Date().getFullYear();
  /** Empty in the application; the app host in the marketing build. "About Sycapooke" always stays local. */
  readonly origin = inject(APP_ORIGIN);

  href(path: string): string | null {
    return externalHref(this.origin, path);
  }
}
