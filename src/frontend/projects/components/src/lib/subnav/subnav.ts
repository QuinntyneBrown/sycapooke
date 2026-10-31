import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';

/** One tab of the subnav. */
export interface SubnavItem {
  label: string;
  path: string;
}

/**
 * Tabs for the pages of one area: a customer's account or the founder's shop management
 * (design system: Subnav). The active route's tab carries aria-current="page".
 */
@Component({
  selector: 'sy-subnav',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './subnav.html',
  styleUrl: './subnav.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SubnavComponent {
  /** The nav landmark's accessible name, for example "Account". */
  readonly label = input.required<string>();
  readonly items = input.required<readonly SubnavItem[]>();
}
