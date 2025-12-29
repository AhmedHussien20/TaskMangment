export interface MenuItem {
  path?: string;
  title?: string;
  headTitle?: string;
  icon?: string;
  type?: string;
  active?: boolean;
  selected?: boolean;
  dirchange?: boolean;
  menutype?: string;
  children?: MenuItem[];
  
  minRoleLevel?: number;
  requiredPermission?: string;
}
