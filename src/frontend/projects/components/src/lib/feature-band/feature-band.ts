import { NgTemplateOutlet } from '@angular/common';
import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** One point of a feature band: a bold title and one line. */
export interface FeaturePoint {
  title: string;
  text: string;
}

/**
 * Marketing band (design system: Feature band): eyebrow, h2, an optional lede, `[actions]`, then
 * up to three points or the projected body (such as sy-steps). Tones: `default` on paper, `sell`
 * (terracotta, cream text, `btn--inverse` buttons) and `story` (cream with a line rule above).
 * `stacked` puts the body under the heading; `closing` marks the page's last sell band.
 */
@Component({
  selector: 'sy-feature-band',
  imports: [NgTemplateOutlet],
  templateUrl: './feature-band.html',
  styleUrl: './feature-band.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FeatureBandComponent {
  readonly eyebrow = input('');
  /** The h2 text; leave empty to project rich text as `[title]` instead. */
  readonly heading = input.required<string>();
  readonly headingId = input.required<string>();
  readonly lede = input('');
  readonly points = input<FeaturePoint[]>([]);
  readonly tone = input<'default' | 'sell' | 'story'>('default');
  readonly stacked = input(false);
  readonly closing = input(false);
}
