export interface Student {
  id: number;
  fullName: string;
  email?: string;
  mobile?: string;
  offerCount: number;
}

export interface StudentAddEdit {
  fullName: string;
  email?: string;
  mobile?: string;
}

export interface StudentPagedResponse {
  data: Student[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}