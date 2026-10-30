import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { SizeTagComponent } from '../size-tag/size-tag';

/** A listing photo as a picture: WebP and JPEG srcsets with the original dimensions (L2-059). */
export interface ListingCardPhoto {
  width: number;
  height: number;
  alt: string;
  src: string;
  webp: string;
  jpeg: string;
}

/** What one card shows. price is already formatted for display, for example "$85". */
export interface ListingCardView {
  href: string;
  title: string;
  brand: string;
  sizeLabel: string;
  condition: string;
  price: string;
  reserved: boolean;
  photo: ListingCardPhoto | null;
}

/**
 * One listing in a grid (design system: Listing card): a square cream photo well carrying the
 * category pill, or a "Reserved" pill when the item is in a checkout; then the serif title, whose
 * link is stretched over the whole card, the brand and condition line, and the terracotta serif
 * price. Text is bound as text, never as HTML.
 */
@Component({
  selector: 'sy-listing-card',
  imports: [RouterLink, SizeTagComponent],
  templateUrl: './listing-card.html',
  styleUrl: './listing-card.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListingCardComponent {
  readonly card = input.required<ListingCardView>();
  /** Above the first screen: load eagerly with high priority; otherwise lazily. */
  readonly priority = input(false);

  /** The width a card fills at each breakpoint, so a 375 px phone picks the 400 w version. */
  readonly sizes = '(min-width: 1200px) 23vw, (min-width: 992px) 31vw, (min-width: 576px) 48vw, 100vw';
}
