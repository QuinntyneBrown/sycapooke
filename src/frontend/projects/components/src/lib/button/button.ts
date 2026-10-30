import { Directive, input } from '@angular/core';

/**
 * Styles a native button or link as a Sycapooke button (design system: Button). Variants primary,
 * secondary (default), ghost, link and danger; sizes sm, md and lg; block fills the row; busy
 * sets aria-busy and keeps the name.
 */
@Directive({
  selector: '[syButton]',
  host: {
    class: 'btn',
    '[class.btn--primary]': "syButton() === 'primary'",
    '[class.btn--ghost]': "syButton() === 'ghost'",
    '[class.btn--link]': "syButton() === 'link'",
    '[class.btn--danger]': "syButton() === 'danger'",
    '[class.btn--sm]': "size() === 'sm'",
    '[class.btn--lg]': "size() === 'lg'",
    '[class.btn--block]': 'block()',
    '[attr.aria-busy]': "busy() ? 'true' : null",
  },
})
export class ButtonDirective {
  readonly syButton = input<'' | 'primary' | 'ghost' | 'link' | 'danger'>('');
  readonly size = input<'sm' | 'md' | 'lg'>('md');
  readonly block = input(false);
  readonly busy = input(false);
}
