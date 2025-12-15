import {
  Component,
  ElementRef,
  ViewChild,
  AfterViewInit,
  OnInit
} from '@angular/core';

import { FullCalendarModule } from '@fullcalendar/angular'; 
import dayGridPlugin from '@fullcalendar/daygrid';
import timeGridPlugin from '@fullcalendar/timegrid';
import interactionPlugin, { DateClickArg, Draggable, EventResizeDoneArg } from '@fullcalendar/interaction';

import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { NgbModule, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { OverlayscrollbarsModule } from 'overlayscrollbars-ngx';
import { CalendarModule } from 'angular-calendar';

import { SharedModule } from 'app/shared/shared.module';
import { TranslateService } from '@ngx-translate/core';
 
import { CalendarOptions, EventClickArg, EventDropArg } from '@fullcalendar/core/index.js'; 
import { CalendarEventGetDto, CalendarEventType, CalendarEventUpsertDto } from 'app/core/models/event/calendar';
import { CommonModule } from '@angular/common';
import { EventCreateModalComponent } from './event-create-modal/event-create-modal.component';
import { CalendarEventService } from 'app/core/services/calendar-events.service';

type CategoryKey =
  | 'calendar'
  | 'birthday'
  | 'holiday'
  | 'office'
  | 'other'
  | 'festival'
  | 'timeline';

interface CategoryItem {
  key: CategoryKey;
  labelKey: string;     // translate key
  className: string;    // FullCalendar className
  borderClass: string;  // left panel style
}

interface ActivityItem {
  title: string;
  dateText: string;
  badgeText: string;
  badgeClass: string;
  description: string;
}

@Component({
  selector: 'app-event-calender',
  standalone: true,
  imports: [
    CommonModule,
    SharedModule,
    OverlayscrollbarsModule,
    CalendarModule,
    FormsModule,
    ReactiveFormsModule,
    NgbModule,
    FullCalendarModule,
    RouterModule
  ],
  templateUrl: './event-calender.component.html',
  styleUrl: './event-calender.component.scss'
})
export class EventCalenderComponent implements OnInit, AfterViewInit {
  @ViewChild('external', { static: false }) external!: ElementRef;
 
  categories: CategoryItem[] = [
  { key: 'calendar', labelKey: 'CALENDAR.EVENT_TYPE.Meeting', className: 'bg-primary border border-primary', borderClass: 'bg-primary border border-primary' },
  { key: 'birthday', labelKey: 'CALENDAR.EVENT_TYPE.Birthday', className: 'bg-warning border border-warning', borderClass: 'bg-warning border border-warning' },
  { key: 'holiday', labelKey: 'CALENDAR.EVENT_TYPE.Holiday', className: 'bg-success border border-success', borderClass: 'bg-success border border-success' },
  { key: 'office',  labelKey: 'CALENDAR.EVENT_TYPE.Appointment', className: 'bg-info border border-info', borderClass: 'bg-info border border-info' },
  { key: 'other',   labelKey: 'CALENDAR.EVENT_TYPE.Reminder', className: 'bg-secondary border border-secondary', borderClass: 'bg-secondary border border-secondary' },
  { key: 'festival',labelKey: 'CALENDAR.EVENT_TYPE.Anniversar', className: 'bg-danger border border-danger', borderClass: 'bg-danger border border-danger' },
  { key: 'timeline',labelKey: 'CALENDAR.EVENT_TYPE.TaskDeadline', className: 'bg-teal border border-teal', borderClass: 'bg-teal border border-teal' }
];


  // fullcalendar events
  calendarEvents: any[] = [];

  // activity panel
  activity: ActivityItem[] = [];
  activityLoading = false;

  loading = false;

  // options
  calendarOptions: CalendarOptions = {
    plugins: [dayGridPlugin, timeGridPlugin, interactionPlugin],
    initialView: 'dayGridMonth',
    headerToolbar: {
      left: 'prev,next today',
      center: 'title',
      right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek'
    },
    navLinks: true,
    editable: true,
    selectable: true,
    selectMirror: true,
    droppable: true,
    weekends: true,
    dayMaxEvents: true,
 
    events: [],

    dateClick: (arg) => this.onDateClick(arg),
    eventClick: (arg) => this.onEventClick(arg),
    eventDrop: (arg) => this.onEventDrop(arg),
    eventResize: (arg) => this.onEventResize(arg)
  };

  constructor(
    private calendarService: CalendarEventService,
    private translate: TranslateService,
    private modal: NgbModal
  ) {}

  ngOnInit(): void {
    this.loadEvents();
  }

  ngAfterViewInit(): void { 
    if (!this.external?.nativeElement) return;

    new Draggable(this.external.nativeElement, {
      itemSelector: '.fc-event',
      eventData: (eventEl: HTMLElement) => {
        const title = eventEl.innerText.trim();
        const cls = eventEl.getAttribute('data-class') || '';
        return {
          title,
          className: cls + ' overflow-hidden'
        };
      }
    });
  }
 
 loadEvents(): void {
  this.loading = true;

  this.calendarService.getAll({
    pageIndex: 1,
    pageSize: 100
  }).subscribe({
    next: (res) => {

      const list: CalendarEventGetDto[] = res.data?.data ?? [];

      this.calendarEvents = list.map((e, index) => {
        const isAllDay = e.allDay === true;

        return {
          id: (e.id ?? index + 1).toString(),  
          title: e.title,

          start: isAllDay
            ? e.startDate.split('T')[0]
            : e.startDate,

          end: isAllDay && e.endDate
            ? e.endDate.split('T')[0]
            : e.endDate ?? undefined,

          allDay: isAllDay,

          className: this.resolveEventClass(e),

          extendedProps: {
            description: e.description,
            relatedTaskId: e.relatedTaskId,
            relatedTaskTitle: e.relatedTaskTitle,
            companyId: e.companyId
          }
        };
      });

      this.calendarOptions = {
        ...this.calendarOptions,
        events: [...this.calendarEvents]
      };

      this.loading = false;
      this.buildActivity(list);

    },
    error: () => {
      this.loading = false;
    }
  });
}



 

  private mapDtoToEvent(dto: CalendarEventGetDto, fallbackIndex: number): any {
    const idValue = (dto.id ?? fallbackIndex + 1).toString();

    return {
      id: idValue,
      title: dto.title,
      start: dto.startDate,
      end: dto.endDate ?? undefined,
      allDay: dto.allDay,
      className: this.resolveClass(dto),
      extendedProps: {
        description: dto.description,
        relatedTaskId: dto.relatedTaskId,
        relatedTaskTitle: dto.relatedTaskTitle,
        companyId: dto.companyId
      }
    };
  }

private resolveEventClass(e: CalendarEventGetDto): string {
  switch (e.eventType) {
    case CalendarEventType.Meeting:
      return 'bg-secondary-transparent';
    case CalendarEventType.Appointment:
      return 'bg-success-transparent';
    case CalendarEventType.TaskDeadline:
      return 'bg-info-transparent';
    case CalendarEventType.Holiday:
      return 'bg-danger-transparent';
    case CalendarEventType.Reminder:
      return 'bg-teal-transparent';
    case CalendarEventType.Birthday:
      return 'bg-warning-transparent';
    case CalendarEventType.Anniversar:
      return 'bg-success-transparent';
    default:
      return 'bg-warning-transparent';
  }
}


  private resolveClass(dto: CalendarEventGetDto): string {
    if (dto.relatedTaskId) return 'bg-primary-transparent';
    if (dto.allDay) return 'bg-success-transparent';
    return 'bg-info-transparent';
  }

  // ========= ACTIVITY =========
 private buildActivity(list: CalendarEventGetDto[]): void {
  this.activityLoading = true;

  const today = new Date();
  today.setHours(0, 0, 0, 0); // بداية اليوم

  // فلترة الأحداث لتكون اليوم فقط
  const todayEvents = list.filter(e => {
    const eventDate = new Date(e.startDate);
    eventDate.setHours(0, 0, 0, 0);
    return eventDate.getTime() === today.getTime();
  });

  // ترتيب الأحداث حسب الوقت أو أي معيار آخر
  const sorted = [...todayEvents].sort((a, b) => {
    const ta = new Date(a.startDate).getTime();
    const tb = new Date(b.startDate).getTime();
    return ta - tb; // أصغر وقت أولًا
  });

  // لو عايز بس top 6
  const top = sorted.slice(0, 6);

  this.activity = top.map((e) => {
    const date = new Date(e.startDate);
    const dateText = date.toLocaleDateString(this.getLocale(), {
      weekday: 'long',
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });

    const badge = e.allDay
      ? this.translate.instant('CALENDAR.ALL_DAY')
      : date.toLocaleTimeString(this.getLocale(), { hour: '2-digit', minute: '2-digit' });

    return {
      title: e.title,
      dateText,
      badgeText: badge,
      badgeClass: e.allDay ? 'bg-success' : 'bg-light text-default',
      description: e.description || e.relatedTaskTitle || e.title
    };
  });

  this.activityLoading = false;
}



  private getLocale(): string {
    const lang = this.translate.currentLang || 'en';
    return lang.startsWith('ar') ? 'ar-EG' : 'en-US';
  }

  // ========= CRUD =========

  onDateClick(arg: DateClickArg): void {
  const modalRef = this.modal.open(EventCreateModalComponent, {
    size: 'lg',
    centered: true
  });

  modalRef.componentInstance.startDate = arg.date;

  modalRef.result.then(
    (saved) => {
      if (saved) {
        this.loadEvents(); // refresh calendar
      }
    },
    () => {}
  );
}

  onEventClick(arg: EventClickArg): void {
    const eventId = Number(arg.event.id);
    const ok = confirm(this.translate.instant('CALENDAR.CONFIRM_DELETE'));
    if (!ok) return;

    // لو id مش موجود/مش رقم (لأن API ما بيرجعش Id)
    if (!eventId || Number.isNaN(eventId)) {
      arg.event.remove();
      return;
    }

    this.calendarService.delete(eventId).subscribe({
      next: () => arg.event.remove(),
      error: () => {}
    });
  }

  onEventDrop(arg: EventDropArg): void {
    this.syncEventUpdate(arg.event);
  }

  onEventResize(arg: EventResizeDoneArg): void {
    this.syncEventUpdate(arg.event);
  }

  private syncEventUpdate(event: any): void {
    const eventId = Number(event.id);
    if (!eventId || Number.isNaN(eventId)) return;

    const payload: CalendarEventUpsertDto = {
      title: event.title,
      description: event.extendedProps?.description ?? '',
      startDate: event.start?.toISOString(),
      endDate: event.end ? event.end.toISOString() : null,
      allDay: event.allDay,
      relatedTaskId: event.extendedProps?.relatedTaskId ?? null
    };

    this.calendarService.update(eventId, payload).subscribe({
      next: () => {},
      error: () => {}
    });
  }

  // زر view all
  onViewAllActivity(): void {
    // لو عايز تروح لصفحة list
    // this.router.navigate(['/calendar/activity']);
  }
}
