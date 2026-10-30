import { Routes } from '@angular/router';

const page = () => import('./catalog/catalog-page').then((m) => m.CatalogPage);

/** Lazy routes of the catalog feature area, mounted at /: the catalog, one page per category and the listing detail. */
export const routes: Routes = [
  { path: '', pathMatch: 'full', loadComponent: page, title: 'Sycapooke' },
  { path: 'jewelry', loadComponent: page, data: { pageCategory: 'Jewelry' }, title: 'Jewelry · Sycapooke' },
  { path: 'clothing', loadComponent: page, data: { pageCategory: 'Clothing' }, title: 'Clothing · Sycapooke' },
  { path: 'housewares', loadComponent: page, data: { pageCategory: 'Housewares' }, title: 'Housewares · Sycapooke' },
  { path: 'electronics', loadComponent: page, data: { pageCategory: 'Electronics' }, title: 'Electronics · Sycapooke' },
  {
    path: 'items/:reference',
    loadComponent: () => import('./listing-detail/listing-detail-page').then((m) => m.ListingDetailPage),
    title: 'Item · Sycapooke',
  },
];
