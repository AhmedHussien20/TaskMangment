import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, Route, Router, RouterStateSnapshot, UrlSegment } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthGuard implements CanActivate {
  constructor(private authService: AuthService, private router: Router) {}

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean | Observable<boolean> {
    return this.checkAccess(state.url, route.data);
  }

  canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): boolean {
    return this.checkAccess(state.url, route.data);
  }

  canLoad(route: Route, segments: UrlSegment[]): boolean {
    return this.checkAccess(route.path || '', route.data || {});
  }

  private checkAccess(redirectUrl: string, data: Record<string, unknown>): boolean {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['auth/login'], { queryParams: { returnUrl: redirectUrl } });
      return false;
    }

    const permissions = (data['permissions'] as string[] | undefined) ?? [];
    const requiredPermission = data['requiredPermission'] as string | undefined;
    const minRoleLevel = (data['roleLevel'] as number | undefined) ?? 0;

    if (requiredPermission && !this.authService.hasPermission(requiredPermission)) {
      this.router.navigate(['auth/forbidden']);
      return false;
    }

    if (permissions.length > 0 && !this.authService.hasAnyPermission(...permissions)) {
      this.router.navigate(['auth/forbidden']);
      return false;
    }

    // Dual-read: if route still declares roleLevel and no permission keys, fall back to level.
    if (permissions.length === 0 && !requiredPermission && minRoleLevel > 0) {
      if (this.authService.getRoleLevel() < minRoleLevel) {
        this.router.navigate(['auth/forbidden']);
        return false;
      }
    }

    return true;
  }
}
