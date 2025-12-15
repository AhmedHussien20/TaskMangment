import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap'; 
import { TranslateModule } from '@ngx-translate/core'; 
import { CalendarEventService } from 'app/core/services/calendar-events.service';

@Component({
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TranslateModule],
  templateUrl: './event-create-modal.component.html'
})
export class EventCreateModalComponent {
  @Input() startDate!: Date;
  form!: FormGroup;
  saving = false;

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
      allDay: true
    });

    const iso = this.toInputDate(this.startDate);
    this.form.patchValue({
      startDate: iso,
      endDate: iso
    });
  }

  onSave() {
    if (this.form.invalid) return;

    this.saving = true;

    const payload = {
      ...this.form.value,
      startDate: new Date(this.form.value.startDate!).toISOString(),
      endDate: this.form.value.endDate
        ? new Date(this.form.value.endDate).toISOString()
        : null
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
