import { ChangeDetectionStrategy, Component, computed, inject } from '@angular/core';
import { toSignal } from '@angular/core/rxjs-interop';
import { ActivatedRouteSnapshot, NavigationEnd, ResolveEnd, Router, RouterOutlet } from '@angular/router';
import { filter, map } from 'rxjs';
import { AuthState } from '@sycapooke/api';
import { FooterComponent, LogoComponent, SubnavComponent, SubnavItem, TopBarComponent } from '@sycapooke/components';
import { PayoutBannerComponent } from '@sycapooke/orders';
import { CookieSettingsDialogService } from '@sycapooke/privacy';

/** Shell named by the deepest route's data.shell: 'app' (default), 'auth' or 'admin'. */
type Shell = 'app' | 'auth' | 'admin' | 'marketing';

/** The customer's account tabs (mocks pages/account). */
const CUSTOMER_TABS: readonly SubnavItem[] = [
  { label: 'Overview', path: '/account' },
  { label: 'Purchases', path: '/account/purchases' },
  { label: 'Emails', path: '/account/emails' },
  { label: 'Settings', path: '/account/settings' },
];

/** The founder's shop-management tabs; each joins when its page is built. */
const FOUNDER_TABS: readonly SubnavItem[] = [
  { label: 'Shop', path: '/shop' },
  { label: 'Inventory', path: '/account/listings' },
  { label: 'Emails', path: '/account/emails' },
  { label: 'Settings', path: '/account/settings' },
];

/** True when the deepest route says it belongs to the account and shop-management area. */
function inAccountArea(route: ActivatedRouteSnapshot): boolean {
  let deepest = route;
  while (deepest.firstChild) deepest = deepest.firstChild;
  return deepest.data['area'] === 'account';
}

function shellOf(route: ActivatedRouteSnapshot): Shell {
  let deepest = route;
  while (deepest.firstChild) deepest = deepest.firstChild;
  const shell = deepest.data['shell'];
  return shell === 'auth' || shell === 'admin' || shell === 'marketing' ? shell : 'app';
}

/**
 * App shell: the payout banner, top bar with the member from AuthState, main#main with the router
 * outlet and the footer, whose "Cookie settings" opens the dialog; a top-bar search navigates to the catalog. Routes with data.shell 'auth' render in the auth shell: large logo above a centred card.
 * Routes with data.shell 'admin' render in the admin shell: the admin top bar and no footer.
 * Routes with data.shell 'marketing' get the app shell with a full-width main.marketing (ADR platform/0011).
 */
@Component({
  selector: 'sy-app-shell',
  imports: [RouterOutlet, TopBarComponent, FooterComponent, LogoComponent, PayoutBannerComponent, SubnavComponent],
  templateUrl: './app-shell.html',
  styleUrl: './app-shell.css',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AppShellComponent {
  private readonly router = inject(Router);
  readonly auth = inject(AuthState);
  readonly cookieSettings = inject(CookieSettingsDialogService);

  readonly shell = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof ResolveEnd),
      map((event) => shellOf(event.state.root)),
    ),
    { initialValue: 'app' as Shell },
  );

  /** True on pages of the account area, which show the subnav under the top bar. */
  readonly accountArea = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof ResolveEnd),
      map((event) => inAccountArea(event.state.root)),
    ),
    { initialValue: false },
  );

  /** The founder sees shop management; a customer sees the account tabs. */
  readonly tabs = computed(() => (this.auth.isFounder() ? FOUNDER_TABS : CUSTOMER_TABS));
  readonly tabsLabel = computed(() => (this.auth.isFounder() ? 'Shop management' : 'Account'));

  /** The keyword of the catalog URL, so the top-bar box keeps it. */
  readonly query = toSignal(
    this.router.events.pipe(
      filter((event) => event instanceof NavigationEnd),
      map(() => this.router.parseUrl(this.router.url).queryParamMap.get('q') ?? ''),
    ),
    { initialValue: '' },
  );

  /** A keyword search goes to the catalog; on the catalog it keeps the filters (L2-023). */
  search(term: string): void {
    const onCatalog = this.router.parseUrl(this.router.url).root.children['primary'] === undefined;
    void this.router.navigate(['/'], {
      queryParams: { q: term || null },
      queryParamsHandling: onCatalog ? 'merge' : '',
    });
  }

  constructor() {
    void this.auth.load();
  }
}
