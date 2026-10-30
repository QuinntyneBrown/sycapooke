import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';

import { APP_ORIGIN, externalHref } from '../app-origin';
import { DIALOG_DATA, DialogRef } from '../dialog/dialog-ref';
import { SheetComponent } from '../sheet/sheet';

/** One category link of the primary navigation. */
export interface NavMenuLink {
  label: string;
  path: string;
}

/**
 * The navigation menu sheet the top bar's menu button opens (dialogs/nav-menu): every category,
 * for widths where the top bar hides its inline links. Choosing a link closes the sheet.
 */
@Component({
  selector: 'sy-nav-menu',
  imports: [RouterLink, SheetComponent],
  template: `
    <sy-sheet heading="Menu" closeLabel="Close menu">
      <nav aria-label="Menu">
        <ul class="menu-sheet__links" role="list">
          @for (item of links; track item.path) {
            <li>
              <a [routerLink]="origin ? null : item.path" [attr.href]="href(item.path)" (click)="ref.close()">{{
                item.label
              }}</a>
            </li>
          }
        </ul>
      </nav>
    </sy-sheet>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NavMenuComponent {
  readonly links = inject<readonly NavMenuLink[]>(DIALOG_DATA);
  readonly ref = inject(DialogRef);
  readonly origin = inject(APP_ORIGIN);

  href(path: string): string | null {
    return externalHref(this.origin, path);
  }
}
