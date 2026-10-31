import { DecimalPipe } from '@angular/common';
import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';

/** What the summary shows of the founder: the shop has one seller, so there is no rating to report. */
export interface FounderSummaryView {
  displayName: string;
  completedSalesCount: number;
}

/**
 * Avatar, the founder's name linking to her page, "Founder of Sycapooke · n completed sales", "No
 * reviews yet" and a link to her story (design system: Founder summary, L2-025).
 */
@Component({
  selector: 'sy-founder-summary',
  imports: [DecimalPipe, RouterLink],
  templateUrl: './founder-summary.html',
  styleUrl: './founder-summary.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class FounderSummaryComponent {
  readonly founder = input.required<FounderSummaryView>();

  readonly initials = computed(() =>
    this.founder()
      .displayName.replace(/[^a-z0-9]/gi, '')
      .slice(0, 2)
      .toUpperCase(),
  );
}
