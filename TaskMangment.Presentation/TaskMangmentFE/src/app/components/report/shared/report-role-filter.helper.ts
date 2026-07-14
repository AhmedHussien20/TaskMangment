import { ReportRoleFilterComponent } from './report-role-filter.component';

export function getReportRoleFilterParams(
  roleFilter: ReportRoleFilterComponent | undefined,
  selectedRoleId?: number
): { roleId?: number; roleTitle?: string } {
  if (!roleFilter?.canFilterByRole || !selectedRoleId) {
    return {};
  }

  return {
    roleId: selectedRoleId,
    roleTitle: roleFilter.selectedRoleTitle
  };
}
