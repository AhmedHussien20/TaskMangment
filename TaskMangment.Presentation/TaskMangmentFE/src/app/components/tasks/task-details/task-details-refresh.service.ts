import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class TaskDetailsRefreshService {

  private refreshSubject = new Subject<string>();

  refresh$ = this.refreshSubject.asObservable();

  trigger(source: string) {
    this.refreshSubject.next(source);
  }
}
