import { Routes } from '@angular/router';
import { founderGuard, signedInGuard } from '@sycapooke/api';

/** The catch-all page for any unknown address. */
export const notFoundRoutes: Routes = [
  {
    path: '**',
    loadComponent: () => import('./errors/not-found-page').then((m) => m.NotFoundPage),
    title: 'Page not found · Sycapooke',
  },
];

/**
 * Lazy routes of the accounts feature area, mounted at the root because /join, /join/google,
 * /verify and /sign-in are top-level paths. They and the public /emails/unsubscribe render in the auth shell;
 * the Emails settings page at /account/emails renders in the app shell.
 */
export const routes: Routes = [
  {
    path: 'join',
    loadComponent: () => import('./join/join-page').then((m) => m.JoinPage),
    title: 'Join Sycapooke',
    data: { shell: 'auth' },
  },
  {
    path: 'join/google',
    loadComponent: () => import('./google-join/google-join-page').then((m) => m.GoogleJoinPage),
    title: 'Finish joining · Sycapooke',
    data: { shell: 'auth' },
  },
  {
    path: 'verify',
    loadComponent: () => import('./verify-email/verify-email-page').then((m) => m.VerifyEmailPage),
    title: 'Verify your email · Sycapooke',
    data: { shell: 'auth' },
  },
  {
    path: 'sign-in',
    loadComponent: () => import('./sign-in/sign-in-page').then((m) => m.SignInPage),
    title: 'Sign in · Sycapooke',
    data: { shell: 'auth' },
  },
  {
    path: 'forgot-password',
    loadComponent: () => import('./forgot-password/forgot-password-page').then((m) => m.ForgotPasswordPage),
    title: 'Reset your password · Sycapooke',
    data: { shell: 'auth' },
  },
  {
    path: 'reset-password',
    loadComponent: () => import('./reset-password/reset-password-page').then((m) => m.ResetPasswordPage),
    title: 'Choose a new password · Sycapooke',
    data: { shell: 'auth' },
  },
  {
    path: '403',
    loadComponent: () => import('./errors/forbidden-page').then((m) => m.ForbiddenPage),
    title: 'You can’t open this · Sycapooke',
  },
  {
    path: 'emails/unsubscribe',
    loadComponent: () => import('./unsubscribe/unsubscribe-page').then((m) => m.UnsubscribePage),
    title: 'Unsubscribe · Sycapooke',
    data: { shell: 'auth' },
  },
  {
    path: 'account/emails',
    loadComponent: () =>
      import('./notification-settings/notification-settings-page').then((m) => m.NotificationSettingsPage),
    title: 'Emails · Sycapooke',
    canActivate: [signedInGuard],
    data: { area: 'account' },
  },
  {
    path: 'account',
    loadComponent: () => import('./account/account-page').then((m) => m.AccountPage),
    title: 'Your account · Sycapooke',
    canActivate: [signedInGuard],
    data: { area: 'account' },
  },
  {
    path: 'account/profile',
    loadComponent: () => import('./account/profile-page').then((m) => m.ProfilePage),
    title: 'Account details · Sycapooke',
    canActivate: [signedInGuard],
    data: { area: 'account' },
  },
  {
    path: 'account/settings',
    loadComponent: () => import('./settings/settings-page').then((m) => m.SettingsPage),
    title: 'Settings · Sycapooke',
    canActivate: [signedInGuard],
    data: { area: 'account' },
  },
  {
    path: 'shop/story',
    loadComponent: () => import('./story/story-page').then((m) => m.StoryPage),
    title: 'Story · Sycapooke',
    canActivate: [founderGuard],
    data: { area: 'account' },
  },
];
