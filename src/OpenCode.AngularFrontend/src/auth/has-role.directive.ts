import { Directive, Input, TemplateRef, ViewContainerRef, OnInit, inject, OnDestroy } from '@angular/core';
import { Subject, combineLatest } from 'rxjs';
import { takeUntil, map } from 'rxjs/operators';
import { AuthService } from './auth.service';

@Directive({
  selector: '[appHasRole]',
  standalone: true,
})
export class HasRoleDirective implements OnInit, OnDestroy {
  private templateRef = inject(TemplateRef<any>);
  private viewContainer = inject(ViewContainerRef);
  private auth = inject(AuthService);
  private destroy$ = new Subject<void>();

  private requiredRoles: string[] = [];
  private showIfUnauthenticated = false;

  @Input() set appHasRole(roles: string | string[]) {
    this.requiredRoles = Array.isArray(roles) ? roles : [roles];
  }

  @Input() set appHasRoleShowIfUnauthenticated(value: boolean) {
    this.showIfUnauthenticated = value;
  }

  ngOnInit(): void {
    combineLatest([
      this.auth.isAuthenticated$,
      this.auth.getRoles(),
    ]).pipe(
      takeUntil(this.destroy$),
      map(([isAuthenticated, userRoles]) => {
        if (this.requiredRoles.length === 0) return true;
        if (!isAuthenticated) return this.showIfUnauthenticated;
        return this.requiredRoles.some(r => userRoles.includes(r));
      })
    ).subscribe(hasAccess => {
      if (hasAccess) {
        this.viewContainer.createEmbeddedView(this.templateRef);
      } else {
        this.viewContainer.clear();
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
