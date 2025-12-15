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

export interface CalendarEventGetDto {
  title: string;
  description: string;

  startDate: string;     // ISO
  endDate?: string | null;
  allDay: boolean;

  relatedTaskId?: number | null;
  relatedTaskTitle?: string | null;

  companyId?: number | null;

  id?: number;
}

export interface CalendarEventUpsertDto {
  title?: string | null | undefined;
  description?: string | null | undefined;
  startDate?: string | null | undefined;
  endDate?: string | null | undefined;
  allDay?: boolean | null | undefined;
  relatedTaskId?: number | null | undefined;
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
