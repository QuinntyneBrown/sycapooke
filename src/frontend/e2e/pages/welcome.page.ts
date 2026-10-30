import { Locator, Page } from '@playwright/test';

/** The marketing page at /welcome. */
export class WelcomePage {
  readonly main: Locator;
  readonly heading: Locator;

  constructor(private readonly page: Page) {
    this.main = page.locator('main#main');
    this.heading = this.main.getByRole('heading', { level: 1 });
  }

  async goto(): Promise<void> {
    await this.page.goto('/welcome');
  }

  /** Every link named "Start selling" on the page. */
  sellLinks(): Locator {
    return this.main.getByRole('link', { name: 'Start selling' });
  }

  link(name: string): Locator {
    return this.main.getByRole('link', { name }).first();
  }

  async headingLevels(): Promise<number[]> {
    return this.main
      .locator('h1, h2, h3, h4')
      .evaluateAll((headings) => headings.map((heading) => Number(heading.tagName.slice(1))));
  }
}
