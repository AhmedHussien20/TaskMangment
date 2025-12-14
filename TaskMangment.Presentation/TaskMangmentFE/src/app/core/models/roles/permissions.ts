export interface Permission {
  id: number;
  code: string;
  name: string;
  description?: string;
}

export interface PermissionAddEdit {
  code: string;
  name: string;
  description?: string;
}

export interface PermissionRequest {
  name?: string;
  code?: string;
  pageIndex: number;
  pageSize: number;
  sortColumn: string;
  sortDirection: string;
}

export interface PermissionPagedResponse {
  data: Permission[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}