export interface BranchGetDto {
  id: number;
  name: string;
  areaName?: string;
  address?: string;
  phone?: string;
  mobile?: string;
  fax?: string;
  email?: string;
  managerName?: string;
  responsibleName?: string;
  areaId?: number;
  managerID?: number;
  responsibleID?: number;
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
