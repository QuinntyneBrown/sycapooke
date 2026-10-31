import { Routes } from '@angular/router';
import { founderGuard } from '@sycapooke/api';

/** Lazy routes of the listings feature area, mounted at /: the sell form, the edit form and the seller's listings. */
export const routes: Routes = [
  {
    path: 'shop',
    loadComponent: () => import('./shop/shop-page').then((m) => m.ShopPage),
    title: 'Shop · Sycapooke',
    canActivate: [founderGuard],
    data: { area: 'account' },
  },
  {
    path: 'sell',
    loadComponent: () => import('./sell/sell-page').then((m) => m.SellPage),
    title: 'Sell an item · Sycapooke',
    canActivate: [founderGuard],
  },
  {
    path: 'sell/:reference',
    loadComponent: () => import('./sell/sell-page').then((m) => m.SellPage),
    title: 'Sell an item · Sycapooke',
    canActivate: [founderGuard],
  },
  {
    path: 'items/:reference/edit',
    loadComponent: () => import('./edit-listing/edit-listing-page').then((m) => m.EditListingPage),
    title: 'Edit listing · Sycapooke',
    canActivate: [founderGuard],
  },
  {
    path: 'account/listings',
    loadComponent: () => import('./my-listings/my-listings-page').then((m) => m.MyListingsPage),
    title: 'Inventory · Sycapooke',
    canActivate: [founderGuard],
    data: { area: 'account' },
  },
];
