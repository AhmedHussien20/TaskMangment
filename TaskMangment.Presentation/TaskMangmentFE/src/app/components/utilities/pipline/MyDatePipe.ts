import { Pipe, PipeTransform } from '@angular/core';
import { formatDate } from '@angular/common';

@Pipe({
  name: 'myDate',
  standalone: true
})
export class MyDatePipe implements PipeTransform {

  transform(
    value: Date | string | number | null | undefined, // أضف null و undefined
    format: string = 'dd/MM/yyyy', 
    locale: string = 'ar'
  ): string {
    if (!value) return ''; // هذا يغطي null و undefined و ''
    return formatDate(value, format, locale);
  }
}