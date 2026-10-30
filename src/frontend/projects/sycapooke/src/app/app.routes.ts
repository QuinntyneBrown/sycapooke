import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: 'welcome', loadChildren: () => import('@sycapooke/marketing').then((m) => m.routes) },
  { path: '', loadChildren: () => import('@sycapooke/catalog').then((m) => m.routes) },
  { path: '', loadChildren: () => import('@sycapooke/listings').then((m) => m.routes) },
  { path: '', loadChildren: () => import('@sycapooke/orders').then((m) => m.routes) },
  { path: 'members', loadChildren: () => import('@sycapooke/reviews').then((m) => m.routes) },
  { path: 'privacy', loadChildren: () => import('@sycapooke/privacy').then((m) => m.routes) },
  { path: '', loadChildren: () => import('@sycapooke/accounts').then((m) => m.routes) },
];
