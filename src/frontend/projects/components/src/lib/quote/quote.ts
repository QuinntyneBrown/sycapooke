import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** A member's own words on a mustard rule, serif, with their display name below (design system: Quote); `size="sm"` for dialogs and order pages. */
@Component({
  selector: 'sy-quote',
  templateUrl: './quote.html',
  styleUrl: './quote.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class QuoteComponent {
  readonly text = input.required<string>();
  /** The display name of who wrote it. */
  readonly cite = input.required<string>();
  /** `sm` is the compact quote of dialogs and order pages. */
  readonly size = input<'md' | 'sm'>('md');
}
