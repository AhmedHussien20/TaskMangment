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