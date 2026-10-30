import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

/**
 * The Sycapooke logo as a link home: a terracotta price tag carrying a Fraunces italic "S" beside
 * the Fraunces italic wordmark, with an optional tagline ("Treasures with a story"). The tag keeps
 * its colours in both themes; the string and wordmark follow the text colour, or cream on the
 * `light` variant for inverse surfaces (brand kit: Heirloom logo). Sizes sm, md, lg (design
 * system: Top bar, Footer, Auth layout).
 */
@Component({
  selector: 'sy-logo',
  imports: [RouterLink],
  templateUrl: './logo.html',
  styleUrl: './logo.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogoComponent {
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly variant = input<'dark' | 'light'>('dark');
  readonly showTagline = input(false);
}
