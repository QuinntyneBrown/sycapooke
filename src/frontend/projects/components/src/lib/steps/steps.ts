import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/** One numbered step. */
export interface Step {
  title: string;
  text: string;
}

/** A numbered list: an outlined serif number circle beside each h3 title and its line (design system: Steps). */
@Component({
  selector: 'sy-steps',
  templateUrl: './steps.html',
  styleUrl: './steps.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StepsComponent {
  readonly steps = input.required<Step[]>();
}
