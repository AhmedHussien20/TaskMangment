import { Pipe, PipeTransform } from '@angular/core';
import { formatDate } from '@angular/common';

function toArabicNumbers(input: string): string {
 const arabicNumbers = ['٠','١','٢','٣','٤','٥','٦','٧','٨','٩'];
 return input.replace(/\d/g, d => arabicNumbers[+d]);
}
@Pipe({
  name: 'myDate',
    standalone: true 

})
export class MyDatePipe implements PipeTransform {


transform(value: Date | string | number | null | undefined, format: string = 'd MMMM y', locale: string = 'ar'): string {
  if (!value) return '';
  const formatted = formatDate(value, format, locale);
  return toArabicNumbers(formatted);
}}