export interface Area {
  id: number;
  name: string;
  address: string;
  managerID: number;
  managerName: string | null;
  branchCount: number;
}

export interface AreaRequest {
  name?: string;
  companyId?: number;
  pageIndex: number;
  pageSize: number;
  sortColumn: string;
  sortDirection: string;
}

export interface AreaPagedResponse {
  data: Area[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}