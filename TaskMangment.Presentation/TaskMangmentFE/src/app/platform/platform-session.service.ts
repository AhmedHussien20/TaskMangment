import { Injectable } from '@angular/core';

export interface PlatformSession {
  userId: number;
  fullName: string;
  email: string;
  role: string;
  organizationId?: number;
  organizationName?: string;
  token: string;
}

@Injectable({ providedIn: 'root' })
export class PlatformSessionService {
  get current(): PlatformSession | null {
    const raw = localStorage.getItem('platformUser');
    const token = localStorage.getItem('platformToken');
    if (!raw || !token) return null;
    try {
      return JSON.parse(raw) as PlatformSession;
    } catch {
      return null;
    }
  }

  save(session: PlatformSession): void {
    localStorage.setItem('platformToken', session.token);
    localStorage.setItem('platformUser', JSON.stringify(session));
  }

  clear(): void {
    localStorage.removeItem('platformToken');
    localStorage.removeItem('platformUser');
    document.documentElement.removeAttribute('data-toggled');
  }

  get homePath(): string {
    const role = this.current?.role;
    if (role === 'SuperAdmin') return '/platform/admins';
    if (role === 'PlatformAdmin') return '/platform/institutes';
    return '/platform/trainees';
  }

  get isSuperAdmin(): boolean {
    return this.current?.role === 'SuperAdmin';
  }

  get isPlatformAdmin(): boolean {
    return this.current?.role === 'PlatformAdmin';
  }

  get isOrganizationAdmin(): boolean {
    return this.current?.role === 'OrganizationAdmin';
  }

  get roleLabel(): string {
    if (this.isSuperAdmin) return 'PLATFORM.PLATFORM_ADMIN';
    if (this.isPlatformAdmin) return 'PLATFORM.STAFF_ADMIN';
    return '';
  }
}
