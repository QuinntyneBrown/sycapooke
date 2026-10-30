import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { AuthState } from '@sycapooke/api';
import {
  ButtonDirective,
  DialogService,
  FeatureBandComponent,
  FeaturePoint,
  MarketingHeroComponent,
  QuoteComponent,
  Step,
  StepsComponent,
  VideoPosterComponent,
} from '@sycapooke/components';

import { PitchVideoDialog } from './pitch-video-dialog';

/**
 * Public marketing page at /welcome (L2-078 to L2-083, L2-089 to L2-091). The hero shows the pitch
 * video's poster only; pressing it opens the video in a dialog, so nothing loads or plays until the
 * visitor asks. The meta description and link-preview tags are static in index.html
 * (index.marketing.html for the marketing build) because crawlers do not run scripts. Static copy,
 * no Sycapooke API call of its own; Start selling goes to /join for a visitor and /sell for a
 * member. Copy follows ADR general/0005: every listing is vintage, at least 20 years old, in a warm
 * and respectful voice.
 */
@Component({
  selector: 'sy-welcome-page',
  imports: [
    RouterLink,
    ButtonDirective,
    MarketingHeroComponent,
    VideoPosterComponent,
    FeatureBandComponent,
    StepsComponent,
    QuoteComponent,
  ],
  templateUrl: './welcome-page.html',
  styleUrl: './welcome-page.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class WelcomePage {
  private readonly auth = inject(AuthState);
  private readonly dialogs = inject(DialogService);

  readonly sellHref = computed(() => (this.auth.member() ? '/sell' : '/join'));

  readonly shopperPoints: FeaturePoint[] = [
    { title: 'Four focused categories', text: 'Jewelry, clothing, housewares and electronics. No clutter.' },
    { title: 'Listed by a real person', text: 'Every listing is a little piece of somebody’s story.' },
    { title: 'Honest condition', text: 'Five grades, and flaws named up front.' },
  ];
  readonly sellerPoints: FeaturePoint[] = [
    {
      title: 'Big buttons, clear steps',
      text: 'Selling a lifetime of belongings should feel respectful — not rushed.',
    },
    { title: 'A real person to call', text: 'If you get stuck, call 1-800-555-0123.' },
  ];
  readonly steps: Step[] = [
    { title: 'List it', text: 'Add photos, a price and the condition.' },
    { title: 'Sell it', text: 'A buyer pays. You ship within 5 business days.' },
    { title: 'Get paid', text: 'The order completes and your payout is on its way.' },
  ];
  openVideo(): void {
    this.dialogs.open(PitchVideoDialog);
  }

  readonly founderQuote =
    'It’s the made-up word I whispered to my son when he was a baby. When it came time to clear out our family home, I realized every teacup and brooch deserved that same word — and someone who’d see it too.';
}
