export interface EmployeeTypeGetDto {
  id: number;
  code: string;
  nameEn: string;
  nameAr: string;
  seesAllTypesInBranchScope: boolean;
  employeeCount: number;
}

export interface EmployeeTypeAddEditDto {
  code: string;
  nameEn: string;
  nameAr: string;
  seesAllTypesInBranchScope: boolean;
}
