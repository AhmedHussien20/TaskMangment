import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { CalendarEventGetDto, CalendarEventType} from 'app/core/models/event/calendar';
import { CalendarEventService } from 'app/core/services/calendar-events.service';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './event-create-modal.component.html'
})
export class EventCreateModalComponent {
  @Input() startDate!: Date;
  @Input() event?: CalendarEventGetDto;
  @Input() isEdit = false;
  @Input() preselectedEventType?: CalendarEventType;
  
  form!: FormGroup;
  saving = false;

  CalendarEventType = CalendarEventType;

  reminderOptions = [
    { value: 15, label: 'CALENDAR.15MIN' },
    { value: 30, label: 'CALENDAR.30MIN' },
    { value: 60, label: 'CALENDAR.60MIN' }
  ];

  eventTypes = Object.keys(CalendarEventType)
    .filter(k => !isNaN(Number(CalendarEventType[k as any]))) 
    .map(k => ({
      value: CalendarEventType[k as keyof typeof CalendarEventType],
      label: 'CALENDAR.EVENT_TYPE.' + k.toUpperCase()
    }));

  constructor(
    private fb: FormBuilder,
    private calendarService: CalendarEventService,
    public activeModal: NgbActiveModal
  ) {}

  ngOnInit(): void {
    this.form = this.fb.nonNullable.group({
      title: ['', Validators.required],
      description: [''],
      startDate: ['', Validators.required],
      endDate: [''],
      allDay: [true],
      eventType: [CalendarEventType.Reminder, Validators.required],
      reminder: [15]
    });

    if (this.isEdit && this.event) {
      this.form.patchValue({
        title: this.event.title,
        description: this.event.description,
        startDate: this.formatDateLocal(new Date(this.event.startDate)),
        endDate: this.event.endDate ? this.formatDateLocal(new Date(this.event.endDate)) : '',
        allDay: this.event.allDay,
        eventType: this.event.eventType,
        reminder: this.event.reminder || 15
      });
    } else {
      const dateStr = this.formatDateLocal(this.startDate || new Date());
      this.form.patchValue({ 
        startDate: dateStr, 
        endDate: dateStr,
        eventType: this.preselectedEventType || CalendarEventType.Reminder
      });
    }
  }

  private formatDateLocal(date: Date): string {
    const d = new Date(date);
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }

  onSave() {
    if (this.form.invalid) return;

    this.saving = true;

    const payload: any = {
      title: this.form.value.title,
      description: this.form.value.description,
      startDate: this.form.value.startDate + ':00',
      endDate: this.form.value.endDate ? this.form.value.endDate + ':00' : null,
      allDay: this.form.value.allDay,
      eventType: Number(this.form.value.eventType),
      reminder: Number(this.form.value.reminder)
    };

    if (this.isEdit && this.event?.id) {
      this.calendarService.update(this.event.id, payload).subscribe({
        next: () => {
          this.activeModal.close(true);
        },
        error: () => {
          this.saving = false;
        }
      });
    } else {
      this.calendarService.create(payload).subscribe({
        next: () => {
          this.activeModal.close(true);
        },
        error: () => {
          this.saving = false;
        }
      });
    }
  }

  
}