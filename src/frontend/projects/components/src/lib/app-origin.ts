import { EnvironmentProviders, InjectionToken, makeEnvironmentProviders } from '@angular/core';

/**
 * Origin of the full application, for links that leave the current build. Empty in the application
 * itself, so links stay in-app router links; `https://app.sycapooke.com` in the marketing build, so
 * "Shop the catalog", "Start selling", the category links and the footer go to the application host
 * (ADR platform/0015).
 */
export const APP_ORIGIN = new InjectionToken<string>('APP_ORIGIN', { providedIn: 'root', factory: () => '' });

export function provideAppOrigin(origin: string): EnvironmentProviders {
  return makeEnvironmentProviders([{ provide: APP_ORIGIN, useValue: origin.replace(/\/$/, '') }]);
}

/** `origin + path` when the link leaves this build, or null so the router link applies instead. */
export function externalHref(origin: string, path: string): string | null {
  return origin ? origin + path : null;
}
