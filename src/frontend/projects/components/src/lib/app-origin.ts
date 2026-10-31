import { EnvironmentProviders, InjectionToken, makeEnvironmentProviders } from '@angular/core';

/**
 * Origin of the full application, for links that leave the current build. Empty in the application
 * itself, so links stay in-app router links; the Azure origin in the marketing build, so
 * "Shop the catalog", "Start selling", category links and the footer leave the /welcome/ base
 * and load the application (ADR platform/0016).
 */
export const APP_ORIGIN = new InjectionToken<string>('APP_ORIGIN', { providedIn: 'root', factory: () => '' });

export function provideAppOrigin(origin: string): EnvironmentProviders {
  return makeEnvironmentProviders([{ provide: APP_ORIGIN, useValue: origin.replace(/\/$/, '') }]);
}

/** `origin + path` when the link leaves this build, or null so the router link applies instead. */
export function externalHref(origin: string, path: string): string | null {
  return origin ? origin + path : null;
}
