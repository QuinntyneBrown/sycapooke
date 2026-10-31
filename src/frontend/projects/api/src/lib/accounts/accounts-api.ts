import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { API_BASE_URL } from '../api-base-url';
import { ApiResult, toResult } from '../api-result';

/** Body of POST /api/auth/register (L2-001). */
export interface Registration {
  displayName: string;
  email: string;
  password: string;
  ageConfirmed: boolean;
  termsAccepted: boolean;
}

/** Which third-party sign-ins Sycapooke API has a client for (L2-088). */
export interface SignInProviders {
  google: boolean;
}

/** The Google identity waiting on the finish-joining page (L2-088). */
export interface PendingGoogleJoin {
  email: string;
}

/** The signed-in customer's own private account (L2-004): GET and PATCH /api/account. */
export interface PrivateAccount {
  displayName: string;
  email: string;
  emailVerified: boolean;
}

/** Typed client for registration, email verification and the Google join steps (L2-001, L2-002, L2-088). */
@Injectable({ providedIn: 'root' })
export class AccountsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  providers(): Promise<ApiResult<SignInProviders>> {
    return toResult(this.http.get<SignInProviders>(`${this.baseUrl}/auth/providers`));
  }

  /**
   * Where "Continue with Google" sends the browser: the API's start endpoint, a full navigation,
   * never an XHR. The API keeps only a same-site returnUrl.
   */
  googleStartUrl(returnUrl: string): string {
    return `${this.baseUrl}/auth/google/start?returnUrl=${encodeURIComponent(returnUrl || '/')}`;
  }

  /** The email of the pending Google join held in the join cookie; 410 once it expired or was used. */
  googleJoinPending(): Promise<ApiResult<PendingGoogleJoin>> {
    return toResult(this.http.get<PendingGoogleJoin>(`${this.baseUrl}/auth/google/pending`));
  }

  register(registration: Registration): Promise<ApiResult<unknown>> {
    return toResult(this.http.post(`${this.baseUrl}/auth/register`, registration));
  }

  verifyEmail(token: string): Promise<ApiResult<unknown>> {
    return toResult(this.http.post(`${this.baseUrl}/auth/verify-email`, { token }));
  }

  /** Always resolves ok for a well-formed request, whether or not the address is registered (L2-009). */
  requestPasswordReset(email: string): Promise<ApiResult<unknown>> {
    return toResult(this.http.post(`${this.baseUrl}/auth/password-reset`, { email }));
  }

  /** 400 RESET_LINK_EXPIRED once the link is used or older than an hour; 422 with a `password` error for a weak one. */
  confirmPasswordReset(token: string, password: string): Promise<ApiResult<unknown>> {
    return toResult(this.http.post(`${this.baseUrl}/auth/password-reset/confirm`, { token, password }));
  }

  privateAccount(): Promise<ApiResult<PrivateAccount>> {
    return toResult(this.http.get<PrivateAccount>(`${this.baseUrl}/account`));
  }

  /** 422 with a `displayName` error for a bad or taken name. */
  updatePrivateAccount(displayName: string): Promise<ApiResult<PrivateAccount>> {
    return toResult(this.http.patch<PrivateAccount>(`${this.baseUrl}/account`, { displayName }));
  }

  /** Signs every other device out and keeps this one signed in; 422 `currentPassword` or `newPassword`. */
  changePassword(currentPassword: string, newPassword: string): Promise<ApiResult<unknown>> {
    return toResult(this.http.post(`${this.baseUrl}/account/password`, { currentPassword, newPassword }));
  }

  /** Revokes every session of the member, this one included. */
  signOutEverywhere(): Promise<ApiResult<unknown>> {
    return toResult(this.http.post(`${this.baseUrl}/auth/sign-out-everywhere`, null));
  }

  /** Sends a new link; earlier links stop working. 429 after 3 links in an hour. */
  requestVerificationEmail(email: string): Promise<ApiResult<unknown>> {
    return toResult(this.http.post(`${this.baseUrl}/auth/verification-email`, { email }));
  }
}
