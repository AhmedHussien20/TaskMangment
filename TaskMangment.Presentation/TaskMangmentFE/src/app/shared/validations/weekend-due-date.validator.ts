import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Saudi weekend: Friday (5) and Saturday (6). */
export function isWeekendDueDate(value: Date | string | null | undefined): boolean {
  if (!value) return false;
  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) return false;
  const day = date.getDay();
  return day === 5 || day === 6;
}

export function startOfToday(): Date {
  const d = new Date();
  d.setHours(0, 0, 0, 0);
  return d;
}

export function isPastDueDate(value: Date | string | null | undefined): boolean {
  if (!value) return false;
  const date = value instanceof Date ? value : new Date(value);
  if (Number.isNaN(date.getTime())) return false;
  const day = new Date(date.getFullYear(), date.getMonth(), date.getDate());
  return day < startOfToday();
}

/** Calendar filter: block Friday, Saturday, and any day before today. */
export function taskDueDateCalendarFilter(date: Date | null): boolean {
  if (!date) return false;
  if (isWeekendDueDate(date)) return false;
  if (isPastDueDate(date)) return false;
  return true;
}

export function weekendDueDateValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    return isWeekendDueDate(control.value) ? { weekendDueDate: true } : null;
  };
}

export function notPastDueDateValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    if (!control.value) return null;
    return isPastDueDate(control.value) ? { pastDueDate: true } : null;
  };
}
