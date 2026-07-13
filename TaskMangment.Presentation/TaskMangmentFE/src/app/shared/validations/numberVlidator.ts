import { AbstractControl, ValidatorFn } from "@angular/forms";

export function decimalValidator(): ValidatorFn {
  return (control: AbstractControl) => {

    if (control.value == null || control.value === '')
      return null;

    

    return isNaN(control.value) ? { decimal: true } : null;
  };
}

export function penaltyAmountValidator(min = 50): ValidatorFn {
  return (control: AbstractControl) => {
    const value = control.value;
    if (value == null || value === '')
      return null;

    const num = Number(value);
    if (isNaN(num))
      return { decimal: true };

    return num >= min ? null : { minPenalty: { min, actual: num } };
  };
}
