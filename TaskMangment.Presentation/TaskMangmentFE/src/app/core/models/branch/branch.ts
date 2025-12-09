export interface Branch {
  id: number;
  name: string;
  areaName: string;
  managerName: string;
  responsibleName: string;
}

export interface BranchRequest {
  name?: string;
  companyId?: number;
  areaId?: number;
  pageIndex: number;
  pageSize: number;
  sortColumn: string;
  sortDirection: string;
}

export interface BranchPagedResponse {
  data: Branch[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}