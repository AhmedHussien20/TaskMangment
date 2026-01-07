export interface LeaveTypeGetDto {
  id: number;
  nameAr: string;
  nameEn: string;
  isPaid: boolean;
  maxDaysPerYear?: number | null;
}

export interface LeaveTypeAddEditDto {
  nameAr: string;
  nameEn: string;
  isPaid: boolean;
  maxDaysPerYear?: number | null;
}
