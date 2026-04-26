import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import { OidcSecurityService } from 'angular-auth-oidc-client';
import { filter, tap } from 'rxjs';

@Component({
  selector: 'app-callback',
  standalone: true,
  template: `
    <div class="p-8 text-center">
      <p>{{ error || 'Completing login...' }}</p>
    </div>
  `,
})
export class CallbackComponent implements OnInit {
  private oidc = inject(OidcSecurityService);
  private router = inject(Router);
  error: string | null = null;

  ngOnInit(): void {
    this.oidc.checkAuth().pipe(
      filter(({ isAuthenticated }) => isAuthenticated !== undefined),
      tap(({ isAuthenticated }) => {
        if (isAuthenticated) {
          this.router.navigate(['/'], { replaceUrl: true });
        }
      }),
    ).subscribe({
      error: (err) => {
        this.error = err?.message || 'Authentication failed';
      },
    });
  }
}
