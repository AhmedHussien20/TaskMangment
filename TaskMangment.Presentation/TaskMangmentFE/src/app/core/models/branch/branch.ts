export interface BranchGetDto {
  id: number;
  name: string;
  areaName?: string;
  managerName?: string;
  responsibleName?: string;
}

export interface BranchAddEditDto {
  name: string;
  address?: string;
  phone?: string;
  mobile?: string;
  fax?: string;
  email?: string;
  areaId?: number;
  managerId?: number;
  responsibleId?: number;
}

export interface BranchPagedResponse {
  data: BranchGetDto[];
  totalCount: number;
  pageIndex: number;
  pageSize: number;
}
