import { Injectable } from "@angular/core";
import { HttpClient } from "@microsoft/signalr";
import { ApiResponse } from "../models/event/calendar";
import { ApiService } from "./api.service";
import { BaseResponse } from "app/models/base.response.model";
import { Observable } from "rxjs";
import { Notification } from "../models/notification/notification";
import { HeaderNotification } from "app/shared/components/header/header.component";

@Injectable({ providedIn: 'root' })
export class NotificationApiService {
    private readonly service = 'notifications';

    constructor(  private api: ApiService) { }
  
    getUnread(): Observable<BaseResponse<HeaderNotification[]>> {
        return this.api.get<BaseResponse<HeaderNotification[]>>(this.service,'unread');
    }

    markAsRead(id: number): Observable<BaseResponse<any>> {
        return this.api.post<BaseResponse<any>>(this.service, `${id}/read`, {});
    }
}
 