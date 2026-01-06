export interface BaseApiRequest {
  id?: number | null;
  searchKey?: string | null;
  pageIndex?: number;     // default 1
  pageSize?: number;      // default 20
  sortColumn?: string;    // default "Id"
  sortDirection?: 'ASC' | 'DESC' | string;
  bypassCache?: boolean;  // default false
}

export interface CalendarEventRequest extends BaseApiRequest {}

export enum CalendarEventType {
  Meeting = 1,
  Appointment = 2,
  TaskDeadline = 3,
  Holiday = 4,
  Reminder = 5,
  Birthday = 6,
  Anniversar = 7,
  Comment = 8
}
export interface CalendarEventGetDto {
  title: string;
  description: string;
  startDate: string;     // ISO
  endDate?: string | null;
  allDay: boolean;
  relatedTaskId?: number | null;
  relatedTaskTitle?: string | null;
  companyId?: number | null;
  id: number;
  eventType: CalendarEventType;
  reminder?: number | null;
}

export interface CalendarEventUpsertDto {
  title?: string | null | undefined;
  description?: string | null | undefined;
  startDate?: string | null | undefined;
  endDate?: string | null | undefined;
  allDay?: boolean | null | undefined;
  relatedTaskId?: number | null | undefined;
  eventType?: CalendarEventType | null | undefined;
  reminder?: number | null | undefined;
}


export interface ApiResponse<T> {
  isSuccess: boolean;
  message?: string | null;
  data: T;
  errors?: string[] | null;
}

export interface CalendarEventFormModel {
  title: string;
  description?: string;
  startDate: string;
  endDate?: string | null;
  allDay: boolean;
  relatedTaskId?: number | null;
}
export interface CalendarPagedResponse {
  data: CalendarEventGetDto[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}