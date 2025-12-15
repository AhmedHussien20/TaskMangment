import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from 'app/core/services/api.service';

import { BaseResponse } from 'app/models/base.response.model';
import { CalendarEventGetDto, CalendarEventRequest, CalendarEventUpsertDto } from '../models/event/calendar';

@Injectable({
  providedIn: 'root'
})
export class CalendarEventService {

  private readonly service = 'CalenderEvents';

  constructor(private api: ApiService) {}

 
  getAll(request?: CalendarEventRequest): Observable<BaseResponse<CalendarEventGetDto[]>> {
    const query = request ? `?${this.buildQuery(request)}` : '';
    return this.api.get<BaseResponse<CalendarEventGetDto[]>>(this.service, query);
  }

 
  getById(id: number): Observable<BaseResponse<CalendarEventGetDto>> {
    return this.api.get<BaseResponse<CalendarEventGetDto>>(this.service, `${id}`);
  }

 
  create(model: CalendarEventUpsertDto): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

 
  update(id: number, model: CalendarEventUpsertDto): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

 
  delete(id: number): Observable<BaseResponse<any>> {
    return this.api.delete<BaseResponse<any>>(this.service, `${id}`);
  }

  // 🔹 Build Query String
  private buildQuery(req: CalendarEventRequest): string {
    const params: string[] = [];

    Object.entries(req).forEach(([key, value]) => {
      if (value !== undefined && value !== null && value !== '') {
        params.push(`${key}=${encodeURIComponent(String(value))}`);
      }
    });

    return params.join('&');
  }
}

