import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { PlatformSessionService } from './platform-session.service';

export const platformAuthGuard: CanActivateFn = () => {
  const session = inject(PlatformSessionService);
  const router = inject(Router);
  return session.current ? true : router.createUrlTree(['/platform/login']);
};

export const platformGuestGuard: CanActivateFn = () => {
  const session = inject(PlatformSessionService);
  const router = inject(Router);
  return session.current ? router.createUrlTree([session.homePath]) : true;
};

export const platformHomeGuard: CanActivateFn = () => {
  const session = inject(PlatformSessionService);
  const router = inject(Router);
  if (!session.current) return router.createUrlTree(['/platform/login']);
  return router.createUrlTree([session.homePath]);
};

export const platformRoleGuard: CanActivateFn = (route) => {
  const session = inject(PlatformSessionService);
  const router = inject(Router);
  const current = session.current;
  if (!current) return router.createUrlTree(['/platform/login']);
  const roles = (route.data['roles'] as string[]) || [];
  return roles.includes(current.role) ? true : router.createUrlTree([session.homePath]);
};
