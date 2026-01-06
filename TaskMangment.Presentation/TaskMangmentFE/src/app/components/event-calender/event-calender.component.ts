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
import interactionPlugin, { DateClickArg, Draggable, EventResizeDoneArg, DropArg } from '@fullcalendar/interaction';

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

import { EventApi } from '@fullcalendar/core';
import Swal from 'sweetalert2';
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

  title = 'CALENDAR.TITLE';
  activeitem = 'CALENDAR.TITLE';
  breadcrumbs = [
  'MENU.HOME',
  'MENU.EMPLOYMENT',
  'CALENDAR.TITLE'
];


  @ViewChild('external', { static: false }) external!: ElementRef;
 
  categories = [
    {
      key: 'Meeting',
      labelKey: 'CALENDAR.EVENT_TYPE.MEETING',
      className: 'bg-secondary',
      borderClass: 'bg-secondary',
      eventType: CalendarEventType.Meeting
    },
    {
      key: 'Appointment',
      labelKey: 'CALENDAR.EVENT_TYPE.APPOINTMENT',
      className: 'bg-success',
      borderClass: 'bg-success',
      eventType: CalendarEventType.Appointment
    },
    {
      key: 'TaskDeadline',
      labelKey: 'CALENDAR.EVENT_TYPE.TASKDEADLINE',
      className: 'bg-info',
      borderClass: 'bg-info',
      eventType: CalendarEventType.TaskDeadline
    },
    {
      key: 'Holiday',
      labelKey: 'CALENDAR.EVENT_TYPE.HOLIDAY',
      className: 'bg-danger',
      borderClass: 'bg-danger',
      eventType: CalendarEventType.Holiday
    },
    {
      key: 'Reminder',
      labelKey: 'CALENDAR.EVENT_TYPE.REMINDER',
      className: 'bg-teal',
      borderClass: 'bg-teal',
      eventType: CalendarEventType.Reminder
    },
    {
      key: 'Birthday',
      labelKey: 'CALENDAR.EVENT_TYPE.BIRTHDAY',
      className: 'bg-warning',
      borderClass: 'bg-warning',
      eventType: CalendarEventType.Birthday
    },
    {
      key: 'Anniversar',
      labelKey: 'CALENDAR.EVENT_TYPE.ANNIVERSAR',
      className: 'bg-purple',
      borderClass: 'bg-purple',
      eventType: CalendarEventType.Anniversar
    },
    {
      key: 'Comment',
      labelKey: 'CALENDAR.EVENT_TYPE.COMMENT',
      className: 'bg-secondary',
      borderClass: 'bg-secondary',
      eventType: CalendarEventType.Comment
    }
  ];

  calendarEvents: any[] = [];
  activity: ActivityItem[] = [];
  activityLoading = false;
  loading = false;

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

    dateClick: (arg) => this.onDateClick(arg, null),
    eventClick: (arg) => this.onEventClick(arg),
    eventDrop: (arg) => this.onEventDrop(arg),
    eventResize: (arg) => this.onEventResize(arg),

    drop: (arg: DropArg) => {
      const eventType = Number(arg.draggedEl?.getAttribute('data-event-type'));
      
      const clickArg: DateClickArg = {
        date: arg.date,
        dateStr: arg.date.toISOString(),
        allDay: arg.allDay,
        jsEvent: arg.jsEvent,
        view: arg.view
      } as DateClickArg;
      
      this.onDateClick(clickArg, eventType);
    }
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
    new Draggable(this.external.nativeElement, {
      itemSelector: '.fc-event',
      eventData: (el: HTMLElement) => {
        const eventType = Number(el.getAttribute('data-event-type'));

        return {
          title: el.innerText.trim(),
          className: el.getAttribute('data-class') + ' overflow-hidden',
          extendedProps: {
            eventType
          }
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
              companyId: e.companyId,
              reminder: e.reminder ?? 15,
              eventType: e.eventType
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
        return 'bg-purple-transparent';
      case CalendarEventType.Comment:
        return 'bg-secondary-transparent';
      default:
        return 'bg-primary-transparent';
    }
  }

  private buildActivity(list: CalendarEventGetDto[]): void {
    this.activityLoading = true;

    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const todayEvents = list.filter(e => {
      const eventDate = new Date(e.startDate);
      eventDate.setHours(0, 0, 0, 0);
      return eventDate.getTime() === today.getTime();
    });

    const sorted = [...todayEvents].sort((a, b) => {
      const ta = new Date(a.startDate).getTime();
      const tb = new Date(b.startDate).getTime();
      return ta - tb;
    });

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

  onDateClick(arg: DateClickArg, eventType: CalendarEventType | null): void {
    const modalRef = this.modal.open(EventCreateModalComponent, {
      size: 'lg',
      centered: true
    });

    modalRef.componentInstance.startDate = arg.date;
    
    // إذا كان هناك نوع حدث (من الـ drag) اضبطه
    if (eventType !== null) {
      modalRef.componentInstance.preselectedEventType = eventType;
    }

    modalRef.result.then(
      (saved) => {
        if (saved) {
          this.loadEvents();
        }
      },
      () => {}
    );
  }

 /* onEventClick(arg: EventClickArg): void {
    const eventId = Number(arg.event.id);
    
    if (!eventId || Number.isNaN(eventId)) {
      // إذا كان الحدث غير موجود في قاعدة البيانات (مثل الأحداث المؤقتة)
      return;
    }

    // تحميل بيانات الحدث وفتح مودال التعديل
    this.calendarService.getById(eventId).subscribe({
      next: (res) => {
        const eventData = res.data;
        if (eventData) {
          this.openEditModal(eventData);
        }
      },
      error: () => {
        // إذا حدث خطأ، افتح مودال بالبيانات الأساسية
        this.openEditModalFromCalendarEvent(arg.event);
      }
    });
  }*/
 onEventClick(arg: { event: EventApi }) {
  const event = arg.event;
  const jsEvent = (arg as any).jsEvent as MouseEvent;

  const menu = document.createElement('div');
  menu.classList.add('dropdown-menu', 'show');
  menu.style.position = 'absolute';
  menu.style.zIndex = '9999';

  const scrollX = window.scrollX || window.pageXOffset;
  const scrollY = window.scrollY || window.pageYOffset;

  menu.style.top = `${jsEvent.clientY + scrollY}px`;
  menu.style.left = `${jsEvent.clientX + scrollX}px`;

  const editBtn = document.createElement('button');
  editBtn.classList.add('dropdown-item');
editBtn.innerText = this.translate.instant('CALENDAR.EDIT_EVENT');
  editBtn.onclick = () => {
    this.openEditModalFromCalendarEvent(event);
    document.body.removeChild(menu);
  };

  const deleteBtn = document.createElement('button');
  deleteBtn.classList.add('dropdown-item');
deleteBtn.innerText = this.translate.instant('CALENDAR.CANCEL_EVENT');
  deleteBtn.onclick = () => {
    document.body.removeChild(menu);
    Swal.fire({
      title: this.translate.instant('COMMON.CONFIRM_DELETE_TITLE'),
      text: this.translate.instant('COMMON.CONFIRM_DELETE_TEXT'),
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: this.translate.instant('COMMON.DELETE_BUTTON'),
      cancelButtonText: this.translate.instant('COMMON.CANCEL_BUTTON'),
    }).then((result) => {
      if (result.isConfirmed) {
        this.deleteEvent(event);
      }
    });
  };

  menu.appendChild(editBtn);
  menu.appendChild(deleteBtn);
  document.body.appendChild(menu);

  const removeMenu = () => {
    if (menu.parentElement) {
      document.body.removeChild(menu);
    }
    document.removeEventListener('click', removeMenu);
  };
  setTimeout(() => {
    document.addEventListener('click', removeMenu);
  }, 0);
}

deleteEvent(event: EventApi) {
  const eventId = Number(event.id);
  if (!eventId) return;

  this.calendarService.delete(eventId).subscribe({
    next: () => {
      event.remove();
    }
  });
}


  private openEditModalFromCalendarEvent(event: any): void {
    const modalRef = this.modal.open(EventCreateModalComponent, {
      size: 'lg',
      centered: true
    });

    const tempEvent: CalendarEventGetDto = {
      id: Number(event.id),
      title: event.title,
      description: event.extendedProps?.description || '',
      startDate: event.start?.toISOString() || '',
      endDate: event.end?.toISOString() || null,
      allDay: event.allDay,
      eventType: event.extendedProps?.eventType || CalendarEventType.Reminder,
      relatedTaskId: event.extendedProps?.relatedTaskId || null,
      relatedTaskTitle: event.extendedProps?.relatedTaskTitle || '',
      companyId: event.extendedProps?.companyId || null,
      reminder: event.extendedProps?.reminder || 15
    };

    modalRef.componentInstance.event = tempEvent;
    modalRef.componentInstance.isEdit = true;

    modalRef.result.then(
      (saved) => {
        if (saved) {
          this.loadEvents();
        }
      },
      () => {}
    );
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
      eventType: event.extendedProps?.eventType ?? CalendarEventType.Reminder,
      relatedTaskId: event.extendedProps?.relatedTaskId ?? null,
      reminder: event.extendedProps?.reminder ?? 15
    };

    this.calendarService.update(eventId, payload).subscribe({
      next: () => {},
      error: () => {}
    });
  }

  onViewAllActivity(): void {
    // لو عايز تروح لصفحة list
    // this.router.navigate(['/calendar/activity']);
  }
}