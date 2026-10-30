import { ChangeDetectionStrategy, Component, input } from '@angular/core';

/**
 * One figure with its label, such as "$33.30 Paid out": a serif italic value beside a forest
 * label; group several in a `.stats` stack divided by line rules (design system: Stats row).
 */
@Component({
  selector: 'sy-stat',
  templateUrl: './stat.html',
  styleUrl: './stat.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class StatComponent {
  readonly label = input.required<string>();
  readonly value = input.required<string>();
}
