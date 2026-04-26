import { Injectable, inject } from '@angular/core';
import { CanActivate, Router, UrlTree } from '@angular/router';
import { Observable, map } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class RoleGuard implements CanActivate {
  private auth = inject(AuthService);
  private router = inject(Router);

  canActivate(route: any): Observable<boolean | UrlTree> {
    const requiredRoles: string[] = route.data?.['roles'] ?? [];
    if (requiredRoles.length === 0) {
      return this.auth.isAuthenticated$.pipe(
        map(isAuth => isAuth || this.router.parseUrl('/login'))
      );
    }
    return this.auth.getRoles().pipe(
      map(userRoles => {
        const hasRole = requiredRoles.some(r => userRoles.includes(r));
        return hasRole || this.router.parseUrl('/');
      })
    );
  }
}
