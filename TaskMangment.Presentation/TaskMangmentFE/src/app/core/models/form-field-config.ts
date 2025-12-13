export interface FormFieldConfig {
  type: 'input' | 'select' | 'textarea' | 'date' | 'checkbox';
  inputType?: 'text' | 'number' | 'email' | 'password';
  label: string;
  name: string;
  validations?: any;
  defaultValue?: any;

  options?: any[];
 
  selectType?: 'simple' | 'employee' | 'custom';
  multiple?: boolean;
}
