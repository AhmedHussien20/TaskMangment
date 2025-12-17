export interface DiscountAddEditDto {
  employeeId: number;
  reason: string;
  amount: number;
}


export interface DiscountListDto {
  employeeName: string;
  taskTitle?: string;
  reason: string;
  amount: number;
  createdAt: string;
}

export interface DiscountPagedResponse {
  data: DiscountListDto[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}

export interface EmployeeOption {
  label: string;
  value: number;
  mobile?: string;
  email?: string;
}