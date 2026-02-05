export interface DiscountAddEditDto {
  employeeId: number;
  reason: string;
  amount: number;
}


export interface DiscountGetDto {
  employeeName: string;
  taskId: number;
  taskTitle?: string;
  reason: string;
  amount: number;
  createdDate: string;
 autoDiscount: boolean;
 isRead: boolean | false

}

export interface DiscountPagedResponse {
  data: DiscountGetDto[];
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