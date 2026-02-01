export function toLocalDateOnly(value: any): string | null {
  if (!value) return null;

  if (value instanceof Date) {
    const d = new Date(
      value.getFullYear(),
      value.getMonth(),
      value.getDate(),
      12, 0, 0
    );

    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');

    return `${y}-${m}-${day}`;
  }

  if (typeof value === 'string') {
    return value.slice(0, 10);
  }

  return null;
}
