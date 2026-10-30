import { ChangeDetectionStrategy, Component, computed, input } from '@angular/core';

/** The Sycapooke size mark, "Vintage", with the last word emphasised (design system: Size tag). */
@Component({
  selector: 'sy-size-tag',
  templateUrl: './size-tag.html',
  styleUrl: './size-tag.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SizeTagComponent {
  /** Short size label, for example "Vintage". */
  readonly label = input.required<string>();
  readonly large = input(false);

  readonly prefix = computed(() => this.label().split(' ').slice(0, -1).join(' '));
  readonly value = computed(() => this.label().split(' ').at(-1) ?? '');
}
