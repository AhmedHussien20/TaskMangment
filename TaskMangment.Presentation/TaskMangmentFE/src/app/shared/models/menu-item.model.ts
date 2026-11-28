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

  /** @deprecated Prefer requiredPermission. Dual-read during migration. */
  minRoleLevel?: number;
  requiredPermission?: string;
  requiredPermissions?: string[];
  requiresAccessScope?: boolean;
  functionCode?: number;
}
