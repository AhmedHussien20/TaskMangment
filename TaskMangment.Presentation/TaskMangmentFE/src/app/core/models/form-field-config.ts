import { Observable } from "rxjs";

export interface FormFieldConfig {
  type: 'input' | 'select' | 'textarea' | 'date' | 'checkbox' | 'radio' | 'file' ; 

  inputType?: 'text' | 'number' | 'email' | 'password' | 'url' | 'mobile';
  countryCodes?: {
  label: string;
  value: string;
  maxLength: number;
   regex: RegExp;
}[];
  label: string;
  name: string;
  validations?: any;
  defaultValue?: any;

  options?: any[];

  group?: string; 

  
  selectType?: 'simple' | 'employee' | 'custom';
  multiple?: boolean;
  /** ng-select: max items when multiple is true */
  maxSelectedItems?: number;

  prefix?: string;
  placeholder?: string;
  disabled?: boolean;
  
  showPassword?: boolean;

  accept?: string;        
  maxFiles?: number;      
  maxFileSizeMB?: number;  
  
  errorMessages?: {
    required?: string;
    pattern?: string;
    min?: string;
    max?: string;
    minlength?: string;
    maxlength?: string;
    email?: string;
    [key: string]: string | undefined;
  };

  isPaginated?: boolean;
  searchFunction?: (searchTerm: string, page: number) => Observable<any>;
  isLoading?: boolean;

  /** Mat datepicker: earliest selectable date */
  minDate?: Date | null;
  /** Mat datepicker: latest selectable date */
  maxDate?: Date | null;
  /** Mat datepicker filter — return false to disable a day */
  dateFilter?: (date: Date | null) => boolean;
}
