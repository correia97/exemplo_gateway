import { Injectable } from '@angular/core';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { Observable, map } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  constructor(private oidcSecurityService: OidcSecurityService) {}

  get isAuthenticated$(): Observable<boolean> {
    return this.oidcSecurityService.isAuthenticated$.pipe(
      map(({ isAuthenticated }) => isAuthenticated)
    );
  }

  get userData$(): Observable<any> {
    return this.oidcSecurityService.userData$;
  }

  get accessToken$(): Observable<string> {
    return this.oidcSecurityService.getAccessToken();
  }

  login(): void {
    this.oidcSecurityService.authorize();
  }

  logout(): void {
    this.oidcSecurityService.logoff().subscribe();
  }

  getRoles(): Observable<string[]> {
    return this.userData$.pipe(
      map((userData: any) => {
        if (!userData?.profile) return [];
        const profile = userData.profile;
        const realmRoles: string[] = profile.realm_roles ?? [];
        const resourceRoles: string[] =
          (profile.resource_access as Record<string, { roles?: string[] }>)?.['frontend']?.roles ?? [];
        const topRoles: string[] = profile.roles ?? [];
        return [...new Set([...realmRoles, ...resourceRoles, ...topRoles])];
      })
    );
  }

  hasRole(role: string): Observable<boolean> {
    return this.getRoles().pipe(
      map(roles => roles.includes(role))
    );
  }
}
