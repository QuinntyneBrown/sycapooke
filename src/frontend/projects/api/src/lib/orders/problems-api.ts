import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

import { API_BASE_URL } from '../api-base-url';
import { ApiResult, toResult } from '../api-result';
import { CheckoutItem } from './checkout-api';
import { OrderStatus, Sale } from './orders-api';

export type ProblemReason = 'NotReceived' | 'NotAsDescribed' | 'WrongSize' | 'Counterfeit';

/** The reasons a buyer can give (L2-036), with the labels Report a problem shows. */
export const PROBLEM_REASONS: readonly { value: ProblemReason; label: string }[] = [
  { value: 'NotReceived', label: 'Not received' },
  { value: 'NotAsDescribed', label: 'Not as described' },
  { value: 'WrongSize', label: 'Wrong size' },
  { value: 'Counterfeit', label: 'Counterfeit' },
];

export type ProblemOutcome = 'RefundBuyer' | 'CloseWithoutRefund';

/** One private evidence photo; the URL is served by Sycapooke API to the founder and the customer. */
export interface ProblemPhoto {
  id: string;
  url: string;
}

/** A buyer's problem with an order: the claim, the seller's one reply and the decision (L2-036). */
export interface OrderProblem {
  reference: string;
  reason: ProblemReason;
  reasonLabel: string;
  description: string;
  photos: ProblemPhoto[];
  status: 'Open' | 'Resolved';
  openedAt: string;
  sellerResponse: string | null;
  sellerRespondedAt: string | null;
  outcome: ProblemOutcome | null;
  resolutionReason: string | null;
  resolvedAt: string | null;
}

/** The founder's problem page: the claim, both parties, and the order's facts. */
export interface ProblemDetail {
  problem: OrderProblem;
  buyerDisplayName: string;
  sellerDisplayName: string;
  order: {
    id: string;
    reference: string;
    status: OrderStatus;
    total: string;
    deliveredAt: string | null;
    item: CheckoutItem;
  };
}

/** Typed client for the members' side of an order problem (resolve-order-problems). */
@Injectable({ providedIn: 'root' })
export class ProblemsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  /**
   * The buyer reports a problem with up to 4 photos. 422 PROBLEM_INVALID with reason, description
   * or photos field errors; 413, 415 or 422 for a photo; 409 PROBLEM_WINDOW_CLOSED or PROBLEM_ALREADY_REPORTED.
   */
  reportProblem(
    orderId: string,
    reason: string,
    description: string,
    photos: readonly File[],
  ): Promise<ApiResult<OrderProblem>> {
    const form = new FormData();
    form.append('reason', reason);
    form.append('description', description);
    photos.forEach((photo) => form.append('photos', photo, photo.name));
    return toResult(this.http.post<OrderProblem>(`${this.baseUrl}/orders/${orderId}/problem`, form));
  }

  /** The seller replies once. 422 PROBLEM_RESPONSE_INVALID; 409 PROBLEM_RESPONSE_NOT_ALLOWED. */
  respondToProblem(orderId: string, message: string): Promise<ApiResult<Sale>> {
    return toResult(this.http.post<Sale>(`${this.baseUrl}/orders/${orderId}/problem/response`, { message }));
  }
}

/** One request in the founder's support queue: a customer's problem or a cancellation request. */
export interface SupportRequest {
  reference: string;
  kind: 'Problem' | 'CancellationRequest';
  label: string;
  item: CheckoutItem;
  total: string;
  customer: string;
  at: string;
  status: 'Open' | 'Refunded' | 'Closed';
  /** The page that handles it, for example /shop/problems/P-1001. */
  link: string;
}

export interface SupportQueue {
  items: SupportRequest[];
  openCount: number;
}

/** Typed client for the founder's side (L2-036); 403 FOUNDER_REQUIRED for anyone else. */
@Injectable({ providedIn: 'root' })
export class ShopProblemsApi {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = inject(API_BASE_URL);

  support(): Promise<ApiResult<SupportQueue>> {
    return toResult(this.http.get<SupportQueue>(`${this.baseUrl}/shop/support`));
  }

  getProblem(reference: string): Promise<ApiResult<ProblemDetail>> {
    return toResult(this.http.get<ProblemDetail>(`${this.baseUrl}/shop/problems/${reference}`));
  }

  /**
   * Refund the customer in full or close the problem without a refund, with a reason the customer sees.
   * 422 PROBLEM_RESOLUTION_INVALID; 409 PROBLEM_ALREADY_RESOLVED or ORDER_INVALID_TRANSITION.
   */
  resolve(reference: string, outcome: ProblemOutcome, reason: string): Promise<ApiResult<ProblemDetail>> {
    return toResult(
      this.http.post<ProblemDetail>(`${this.baseUrl}/shop/problems/${reference}/resolution`, { outcome, reason }),
    );
  }
}
