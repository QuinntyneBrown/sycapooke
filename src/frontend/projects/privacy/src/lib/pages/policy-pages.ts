import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonDirective, CenterStateComponent, SizeTagComponent } from '@sycapooke/components';

import { CookieSettingsDialogService } from '../cookie-settings/cookie-settings.service';

/** /privacy (mocks pages/privacy): what the shop keeps about customers, in short rows. */
@Component({
  selector: 'sy-privacy-page',
  imports: [],
  template: `
    <div class="page--narrow">
      <div class="page-header">
        <div class="stack stack--sm">
          <h1>Privacy</h1>
          <p class="subtitle">Updated 1 Oct 2026</p>
        </div>
      </div>
      <ul class="rules-list" role="list">
        @for (row of rows; track row.title) {
          <li class="settings-list__body policy__row">
            <span class="settings-list__title">{{ row.title }}</span>
            <span class="settings-list__meta">{{ row.text }}</span>
          </li>
        }
      </ul>
      <p class="text-sm policy__foot">
        <a href="#cookie-settings" (click)="openCookies($event)">Cookie settings</a>
      </p>
    </div>
  `,
  styleUrl: './policy-pages.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PrivacyPage {
  private readonly cookies = inject(CookieSettingsDialogService);

  readonly rows = [
    {
      title: 'What we collect',
      text: 'Email, sign-in details, display name, shipping addresses, and your purchases.',
    },
    {
      title: 'Never public',
      text: 'Customer accounts are private: your name, email and address never appear on Sycapooke pages. Only the founder’s story is public.',
    },
    { title: 'Encrypted', text: 'Shipping addresses are encrypted at rest. Card numbers never reach us.' },
    { title: 'Erasure', text: 'Close your customer account and we erase your data within 30 days.' },
    { title: 'Cookies', text: 'Only necessary cookies until you accept analytics.' },
  ];

  openCookies(event: Event): void {
    event.preventDefault();
    this.cookies.open();
  }
}

/** /terms (mocks pages/terms). */
@Component({
  selector: 'sy-terms-page',
  imports: [RouterLink],
  template: `
    <div class="page--narrow">
      <div class="page-header">
        <div class="stack stack--sm">
          <h1>Terms of service</h1>
          <p class="subtitle">Effective 1 Oct 2026</p>
        </div>
      </div>
      <ol class="terms-list">
        @for (term of terms; track term) {
          <li>{{ term }}</li>
        }
      </ol>
      <p class="text-sm text-muted policy__foot">
        See also the <a routerLink="/rules">shop policies</a> and <a routerLink="/privacy">privacy</a>.
      </p>
    </div>
  `,
  styleUrl: './policy-pages.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class TermsPage {
  readonly terms = [
    'You must be 18 or older to hold a customer account.',
    'Every piece in the shop is at least 20 years old, sold by its founder.',
    'Condition and flaws are described honestly, with photos of the actual item.',
    'Payment goes through our payment provider straight to the shop’s account.',
    'You can report a problem until 3 days after delivery. The founder handles it herself and can refund in full or explain why not.',
    'Sycapooke operates in the United States only.',
  ];
}

/** /rules (mocks pages/rules): the shop policies. */
@Component({
  selector: 'sy-shop-policies-page',
  imports: [SizeTagComponent],
  template: `
    <div class="page--narrow">
      <div class="page-header">
        <div class="stack stack--sm">
          <h1>Shop policies</h1>
          <p class="subtitle">Short, because it only has to cover one shop.</p>
        </div>
      </div>
      <div class="stack stack--lg">
        <div class="table-wrap">
          <table class="table">
            <caption class="visually-hidden">
              Accepted categories and sizes
            </caption>
            <thead>
              <tr>
                <th scope="col">Category</th>
                <th scope="col">Includes</th>
                <th scope="col">Size</th>
              </tr>
            </thead>
            <tbody>
              @for (row of categories; track row.name) {
                <tr>
                  <th scope="row">{{ row.name }}</th>
                  <td>{{ row.includes }}</td>
                  <td><sy-size-tag label="Vintage" /></td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <ol class="rules-list" role="list">
          @for (rule of rules; track rule.title) {
            <li class="settings-list__body policy__row">
              <span class="settings-list__title">{{ rule.title }}</span>
              <span class="settings-list__meta">{{ rule.text }}</span>
            </li>
          }
        </ol>
      </div>
    </div>
  `,
  styleUrl: './policy-pages.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ShopPoliciesPage {
  readonly categories = [
    {
      name: 'Jewelry',
      includes: 'Necklaces, Brooches and pins, Earrings, Bracelets, Rings, Watches',
    },
    {
      name: 'Clothing',
      includes: 'Dresses, Coats and jackets, Knitwear, Shirts and blouses, Skirts and trousers, Hats and accessories',
    },
    { name: 'Housewares', includes: 'Tableware, Glassware, Ceramics and pottery, Lighting, Linens, Decor' },
    { name: 'Electronics', includes: 'Radios, Cameras, Audio, Telephones, Clocks, Typewriters and office' },
  ];
  readonly rules = [
    {
      title: 'Vintage only',
      text: 'Jewelry, clothing, housewares and electronics. Every piece in the shop is at least 20 years old.',
    },
    {
      title: 'Honest condition',
      text: 'New with tags, New without tags, Used - Excellent, Used - Good, Used - Fair. Used - Fair has its flaws described.',
    },
    { title: 'Ships in 5 business days', text: 'The founder adds tracking when she ships your order.' },
    {
      title: 'Paid straight to the shop',
      text: 'Your payment goes directly to the shop’s own account. There is no middleman.',
    },
    {
      title: 'Problems handled by the founder',
      text: 'Tell the founder within 3 days of delivery. She replies herself and can refund in full or explain why not.',
    },
    { title: 'US only', text: 'Sycapooke ships within the United States.' },
  ];
}

/** /error (mocks pages/server-error): something went wrong on our side. */
@Component({
  selector: 'sy-server-error-page',
  imports: [RouterLink, ButtonDirective, CenterStateComponent],
  template: `
    <sy-center-state code="500" heading="Something went wrong">
      <p class="text-muted">It’s on our side. Your data is safe.</p>
      <a actions syButton="primary" routerLink="/">Back to shop</a>
    </sy-center-state>
  `,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ServerErrorPage {}
