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

  
  form!: FormGroup;
  saving = false;
  isEdit = false;

  // لإنشاء Dropdown من enum
  eventTypes = Object.keys(CalendarEventType)
    .filter(k => !isNaN(Number(CalendarEventType[k as any]))) // فلتر القيم الرقمية
    .map(k => ({
      value: CalendarEventType[k as keyof typeof CalendarEventType],
      label: k
    }));

  constructor(
    private fb: FormBuilder,
    private calendarService: CalendarEventService,
    public activeModal: NgbActiveModal
  ) {}

  ngOnInit(): void {
    this.isEdit = !!this.event;

    this.form = this.fb.nonNullable.group({
      title: ['', Validators.required],
      description: [''],
      startDate: ['', Validators.required],
      endDate: [''],
      allDay: true,
      eventType: [CalendarEventType.Reminder, Validators.required] // ← نوع الحدث
    });

    if (this.isEdit && this.event) {
      this.form.patchValue({
        title: this.event.title,
        description: this.event.description,
        startDate: this.event.startDate, 
        endDate: this.event.endDate ?? '',
        allDay: this.event.allDay,
        eventType: this.event.eventType 
      });
    } else {
      const dateStr = this.formatDateLocal(this.startDate);
    this.form.patchValue({ startDate: dateStr, endDate: dateStr });
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

    const payload = {
  ...this.form.value,
    startDate: this.form.value.startDate, 
  endDate: this.form.value.endDate,
    
  relatedTaskId: this.form.value.relatedTaskId ? Number(this.form.value.relatedTaskId) : null,
  eventType: this.form.value.eventType
};

    this.calendarService.create(payload).subscribe({
      next: () => {
        this.activeModal.close(true);
      },
      error: () => {
        this.saving = false;
      }
    });
  }

  private toInputDate(date: Date): string {
    return new Date(date)
      .toISOString()
      .slice(0, 16);
  }
}
