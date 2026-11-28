from pathlib import Path
import re

path = Path(r'c:\Users\Administrator\source\repos\TaskMangment\TaskMangment.Infrastructure\Services\ReportService.cs')
text = path.read_text(encoding='utf-8')

text = text.replace(
    'await GetScopedEmployeeIdsAsync(currentEmployeeId, roleLevel)',
    'await GetScopedEmployeeIdsAsync(currentEmployeeId)')

text = text.replace(
    'var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) = await GetScopedEmployeeIdsAsync(currentEmployeeId);',
    'var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope, canViewScopedReports, canViewCompanyReports) = await GetScopedEmployeeIdsAsync(currentEmployeeId);')

text = text.replace(
    'ApplyRoleFilter(scopedEmployeeIds, roleId, roleLevel)',
    'ApplyRoleFilter(scopedEmployeeIds, roleId)')

# Pattern: roleId resolve before scope fetch — rewrite common blocks
pattern = re.compile(
    r'var roleId = ResolveRoleFilter\(roleLevel, filter\?\.RoleId\);\s*'
    r'(?P<body>.*?)\s*'
    r'var \(scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope, canViewScopedReports, canViewCompanyReports\) = await GetScopedEmployeeIdsAsync\(currentEmployeeId\);\s*'
    r'scopedEmployeeIds = ApplyRoleFilter\(scopedEmployeeIds, roleId\);',
    re.S)

def repl(m):
    body = m.group('body')
    return (
        'var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope, canViewScopedReports, canViewCompanyReports) = await GetScopedEmployeeIdsAsync(currentEmployeeId);\n'
        f'{body}\n'
        '            var roleId = ResolveRoleFilter(canViewScopedReports || canViewCompanyReports, filter?.RoleId);\n'
        '            scopedEmployeeIds = ApplyRoleFilter(scopedEmployeeIds, roleId);'
    )

text2, n = pattern.subn(repl, text)
print('rewrote resolve blocks:', n)

# Remaining ResolveRoleFilter(roleLevel
text2 = text2.replace(
    'var roleId = ResolveRoleFilter(roleLevel, filter?.RoleId);',
    'var roleId = ResolveRoleFilter(canViewScopedReports || canViewCompanyReports, filter?.RoleId);')

# Employee total discount company gate
text2 = text2.replace(
    'if (roleLevel < 100)\n                return new List<EmployeeTotalDiscountReportDto>();',
    'var (_, _, _, _, _, canViewCompanyReports) = await GetScopedEmployeeIdsAsync(currentEmployeeId);\n'
    '            if (!canViewCompanyReports)\n                return new List<EmployeeTotalDiscountReportDto>();')

# Unpack patterns that discard extras: "var (scopedEmployeeIds, ..." already handled
# Also: "var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope) =" might remain with weird spacing
text2 = re.sub(
    r'var \(scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope\) =',
    'var (scopedEmployeeIds, canViewAllTasks, canViewCreatedTasks, hasAccessScope, canViewScopedReports, canViewCompanyReports) =',
    text2)

path.write_text(text2, encoding='utf-8')
print('ResolveRoleFilter(roleLevel remaining:', text2.count('ResolveRoleFilter(roleLevel'))
print('roleLevel < 100 remaining:', text2.count('roleLevel < 100'))
print('ApplyRoleFilter 3-arg remaining:', text2.count('ApplyRoleFilter(scopedEmployeeIds, roleId, roleLevel)'))
print('4-tuple remaining:', text2.count('canViewAllTasks, canViewCreatedTasks, hasAccessScope) = await'))
