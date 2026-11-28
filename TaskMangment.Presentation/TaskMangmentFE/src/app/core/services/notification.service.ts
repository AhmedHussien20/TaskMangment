import { Injectable } from "@angular/core";
import { ApiService } from "./api.service";
import { BaseResponse } from "app/models/base.response.model";
import { Observable, Subject } from "rxjs";
import { TaskNotification } from "../models/notification/notification";
import { HeaderNotification } from "app/shared/components/header/header.component";

@Injectable({ providedIn: 'root' })
export class NotificationApiService {
    private readonly service = 'notifications';
    private readonly unreadChangedSubject = new Subject<void>();
    readonly unreadChanged$ = this.unreadChangedSubject.asObservable();

    constructor(private api: ApiService) { }

    getUnread(): Observable<BaseResponse<HeaderNotification[]>> {
        return this.api.get<BaseResponse<HeaderNotification[]>>(this.service, 'unread');
    }

    getByTask(taskId: number): Observable<BaseResponse<TaskNotification[]>> {
        return this.api.get<BaseResponse<TaskNotification[]>>(this.service, `by-task/${taskId}`);
    }

    markAsRead(id: number): Observable<BaseResponse<any>> {
        return this.api.post<BaseResponse<any>>(this.service, `${id}/read`, {});
    }

    markAsReadByTask(taskId: number): Observable<BaseResponse<any>> {
        return this.api.post<BaseResponse<any>>(this.service, `by-task/${taskId}/read`, {});
    }

    notifyUnreadChanged(): void {
        this.unreadChangedSubject.next();
    }
}
 