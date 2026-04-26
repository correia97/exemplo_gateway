import { Injectable } from '@angular/core';
import {
  HttpInterceptor,
  HttpRequest,
  HttpHandler,
  HttpEvent,
  HttpErrorResponse,
} from '@angular/common/http';
import { Observable, switchMap, throwError, catchError } from 'rxjs';
import { AuthService } from '../auth/auth.service';
import { DRAGONBALL_API_URL, MUSIC_API_URL } from './env';

const API_BASES = [DRAGONBALL_API_URL, MUSIC_API_URL];

@Injectable()
export class ApiClientInterceptor implements HttpInterceptor {
  constructor(private auth: AuthService) {}

  intercept(
    req: HttpRequest<unknown>,
    next: HttpHandler
  ): Observable<HttpEvent<unknown>> {
    const matches = API_BASES.some(base => req.url.startsWith(base));
    if (!matches) {
      return next.handle(req);
    }

    const correlationId = crypto.randomUUID();
    let headers = req.headers
      .set('X-Correlation-Id', correlationId)
      .set('Content-Type', 'application/json');

    if (req.method !== 'GET') {
      return this.auth.accessToken$.pipe(
        switchMap(token => {
          if (token) {
            headers = headers.set('Authorization', `Bearer ${token}`);
          }
          const authReq = req.clone({ headers });
          return next.handle(authReq).pipe(
            catchError((error: HttpErrorResponse) => {
              const apiError = {
                message: error.error?.message || error.message || `Request failed (${error.status})`,
                statusCode: error.status,
                correlationId,
              };
              return throwError(() => apiError);
            })
          );
        })
      );
    }

    const clonedReq = req.clone({ headers });
    return next.handle(clonedReq).pipe(
      catchError((error: HttpErrorResponse) => {
        const apiError = {
          message: error.error?.message || error.message || `Request failed (${error.status})`,
          statusCode: error.status,
          correlationId,
        };
        return throwError(() => apiError);
      })
    );
  }
}
