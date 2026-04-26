import { ApplicationConfig, ErrorHandler, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideAuth } from 'angular-auth-oidc-client';
import { routes } from './app.routes';
import { authConfig } from '../auth/auth.config';
import { ApiClientInterceptor } from '../api/client.service';
import { GlobalErrorHandler } from '../shared/services/global-error-handler.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi()),
    { provide: HTTP_INTERCEPTORS, useClass: ApiClientInterceptor, multi: true },
    provideAuth(authConfig),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
  ],
};
