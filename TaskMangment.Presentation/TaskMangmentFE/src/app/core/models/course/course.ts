export interface Course {
  id: number;
  title: string;
  description?: string;
  subjects: string[];
  offerCount: number;
}

export interface CourseAddEdit {
  title: string;
  description?: string;
  subjects: string[];
}

export interface CourseRequest {
  searchKey?: string;
  pageIndex: number;
  pageSize: number;
  sortColumn: string;
  sortDirection: string;
}

export interface CoursePagedResponse {
  data: Course[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}