import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthState } from './auth-state';

/**
 * Route guards improve navigation only. Sycapooke API checks every request itself (L2-010): a guard
 * that is bypassed shows an empty page whose calls are refused with 401 or 403.
 */

/** Sends a visitor to sign in and back again afterwards. */
export const signedInGuard: CanActivateFn = async (_route, state) => {
  const auth = inject(AuthState);
  const router = inject(Router);
  await auth.load();
  return auth.member() ? true : router.createUrlTree(['/sign-in'], { queryParams: { returnUrl: state.url } });
};

/** Lets only the founder in; a signed-in customer sees the management 403, a visitor signs in first. */
export const founderGuard: CanActivateFn = async (_route, state) => {
  const auth = inject(AuthState);
  const router = inject(Router);
  await auth.load();
  if (!auth.member()) {
    return router.createUrlTree(['/sign-in'], { queryParams: { returnUrl: state.url } });
  }

  return auth.isFounder() ? true : router.createUrlTree(['/403'], { queryParams: { reason: 'management' } });
};
