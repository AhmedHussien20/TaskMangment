import { CommonModule } from '@angular/common';
import { Component, forwardRef, Input } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-date-picker',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    TranslateModule,
    MatFormFieldModule,
    MatInputModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule
  ],
  templateUrl: './date-picker.component.html',
  styleUrl: './date-picker.component.scss',
  providers: [
    {
      provide: NG_VALUE_ACCESSOR,
      useExisting: forwardRef(() => DatePickerComponent),
      multi: true
    }
  ]
})
export class DatePickerComponent implements ControlValueAccessor {
  // UI
  @Input() labelKey?: string;
  @Input() placeholderKey?: string;
  @Input() appearance: 'fill' | 'outline' = 'outline';
  @Input() size: 'sm' | 'md' = 'sm';

  // Config
  @Input() required = false;
  @Input() disabled = false;

  // ✅ min/max as string (yyyy-MM-dd) OR Date (flexible)
  @Input() min?: string | Date | null;
  @Input() max?: string | Date | null;

  // ✅ output format (default: yyyy-MM-dd)
  @Input() outputFormat: 'yyyy-MM-dd' | 'iso' = 'yyyy-MM-dd';

  // Internal Date for mat-datepicker
  valueDate: Date | null = null;

  private onChange: (val: string | null) => void = () => {};
  private onTouched: () => void = () => {};

  // CVA: receive string from outside
  writeValue(val: string | null | undefined): void {
    if (!val) {
      this.valueDate = null;
      return;
    }
    this.valueDate = this.parseToDate(val);
  }

  registerOnChange(fn: any): void {
    this.onChange = fn;
  }

  registerOnTouched(fn: any): void {
    this.onTouched = fn;
  }

  setDisabledState(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  // called when user changes date in picker
  onDateChange(d: Date | null): void {
    this.valueDate = d;
    this.onTouched();

    const out = d ? this.formatOut(d) : null;
    this.onChange(out);
  }

  // ===== helpers =====

  // parse "yyyy-MM-dd" safely as LOCAL date (no timezone shift)
  private parseToDate(input: string): Date | null {
    // if input looks like yyyy-MM-dd
    const m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(input.trim());
    if (m) {
      const y = Number(m[1]);
      const mo = Number(m[2]) - 1;
      const day = Number(m[3]);
      return new Date(y, mo, day); // local midnight
    }

    // fallback: try Date constructor (iso etc.)
    const d = new Date(input);
    return isNaN(d.getTime()) ? null : d;
  }

  private formatOut(d: Date): string {
    if (this.outputFormat === 'iso') {
      return d.toISOString();
    }
    // yyyy-MM-dd (LOCAL)
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  // min/max Date to bind to matInput
  get minDate(): Date | null {
    if (!this.min) return null;
    return this.min instanceof Date ? this.min : this.parseToDate(this.min) ;
  }

  get maxDate(): Date | null {
    if (!this.max) return null;
    return this.max instanceof Date ? this.max : this.parseToDate(this.max) ;
  }
}
