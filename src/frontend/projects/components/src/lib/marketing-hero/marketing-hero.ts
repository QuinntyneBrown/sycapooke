import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * The landing-page hero on paper (design system: Marketing hero): eyebrow, a headline projected as
 * `[title]` (h1), one lede line, `[actions]` and, in the second column, the `[media]` photo in a
 * rounded 4:5 frame. Flat: no shapes, no gradient.
 */
@Component({
  selector: 'sy-marketing-hero',
  imports: [NgTemplateOutlet],
  templateUrl: './marketing-hero.html',
  styleUrl: './marketing-hero.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MarketingHeroComponent {
  readonly eyebrow = input('');
  readonly lede = input('');
  /** Id of the headline, which labels the section. */
  readonly headingId = input.required<string>();
}
