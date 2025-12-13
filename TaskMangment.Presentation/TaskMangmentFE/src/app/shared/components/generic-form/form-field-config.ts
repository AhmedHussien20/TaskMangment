interface FormFieldConfig {
  type: 'input' | 'select' | 'textarea';
  label: string;
  name: string;
  validations?: any;
  defaultValue?: any;
  options?: { label: string; value: any }[];  // ← هنا نحدد النوع
}
