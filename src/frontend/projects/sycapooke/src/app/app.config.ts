import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter, withComponentInputBinding } from '@angular/router';
import { provideApi } from '@sycapooke/api';
import { provideAppOrigin } from '@sycapooke/components';
import { provideAnalytics } from '@sycapooke/privacy';

import { environment } from '../environments/environment';
import { routes } from './app.routes';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes, withComponentInputBinding()),
    provideApi(environment.apiBaseUrl),
    provideAppOrigin(environment.appOrigin),
    provideAnalytics(environment.analytics),
  ],
};
