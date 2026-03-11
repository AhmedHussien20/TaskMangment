using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.ApiRequests;
using TaskMangment.Application.ApiRequests.Area;
using TaskMangment.Application.Common.ApiRequests.Branch;
using TaskMangment.Application.Common.ApiRequests.Department;
using TaskMangment.Application.Common.ApiRequests.Employee;
using TaskMangment.Application.Common.ApiRequests.Leave;
using TaskMangment.Application.Common.ApiRequests.Role;
using TaskMangment.Application.Common.ApiRequests.Task;

namespace TaskMangment.Infrastructure.Caching
{
    public static class CacheKeys
    {
        public static string TasksVersion(int companyId) => $"v:tasks:{companyId}";

        #region Dashboard cache keys
        public static string DashboardVersion(int companyId) => $"v:dashboard:{companyId}";
        public static string BranchesForFilter(int companyId, int roleLevel, int employeeId)
    => $"dashboard:branches:company{companyId}:role{roleLevel}:emp{employeeId}";

        public static string AdminDashboard(int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:admin:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";

        public static string AdminKpisExtended(int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:kpisx:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";

        public static string AdminDiscounts(int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version, int pageIndex,int pageSize)
            => $"dashboard:discounts:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}:p{pageIndex}:s{pageSize}";

        public static string AdminHighPriority(int companyId, int roleLevel, int? employeeId, int? branchId, int version, int pageIndex,int pageSize)
            => $"dashboard:highprio:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:p{pageIndex}:s{pageSize}";
        public static string UpdatedTodayTasks(int companyId,int roleLevel,int? employeeId,int? branchId,string periodKey,int pageIndex,int pageSize,int version)
            => $"dashboard:todayupd:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}:p{pageIndex}:s{pageSize}";

        public static string CompletedTodayEmployees(int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:completedToday:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";

        public static string PendingCloseRequests(
            int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:pendingClose:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";

        public static string TasksByStatus(int companyId,string status,int roleLevel,int? employeeId,int? branchId,string periodKey,int version,int pageIndex,int pageSize)
        {
            return $"dashboard:tasksByStatus:{companyId}:v{version}:status{status}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}:p{pageIndex}:s{pageSize}";
        }

        public static string CompletedTasksDetails(int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:completedDetails:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";



        #endregion

        #region Tasks list cache keys

        public static string TasksList(int companyId, int roleLevel, int employeeId, TaskRequest request, int version)
        {
            string Norm(string? s) => (s ?? "").Trim().ToLowerInvariant();

            string DateKey(DateTime? d) => d.HasValue ? d.Value.ToString("yyyyMMdd") : "null";

            string IdKey(int? id) => id.HasValue ? id.Value.ToString() : "null";

            string DirectionKey(TaskDirection? dir) => dir.HasValue ? ((int)dir.Value).ToString() : "null";

            var empIds = (request.EmployeeIds == null || request.EmployeeIds.Count == 0)
                ? "null"
                : string.Join(",", request.EmployeeIds.OrderBy(x => x));

            var sortCol = Norm(request.SortColumn);
            var sortDir = Norm(request.SortDirection);

            return
                $"tasks:list:" +
                $"c{companyId}:v{version}:r{roleLevel}:e{employeeId}:" +
                $"p{request.PageIndex}:s{request.PageSize}:" +
                $"sc{sortCol}:{sortDir}:" +
                $"q{Norm(request.searchKey)}:" +
                $"st{IdKey(request.StatusId)}:" +
                $"dir{DirectionKey(request.Direction)}:" +
                $"tgt{IdKey(request.TargetEmployeeId)}:" +
                $"pr{IdKey(request.PriorityId)}:" +
                $"cf{DateKey(request.CreatedFrom)}:ct{DateKey(request.CreatedTo)}:" +
                $"df{DateKey(request.DueFrom)}:dt{DateKey(request.DueTo)}:" +
                $"emps[{empIds}]";
        }
        #endregion


        #region Employees list cache keys
        public static string EmployeesVersion(int companyId) => $"v:employees:{companyId}";
        public static string EmployeesList(int companyId, int employeeId, int roleLevel, EmployeeRequest request, int version)
        {
            string Norm(string? s) => (s ?? "").Trim().ToLowerInvariant();
            string IdKey(int? id) => id.HasValue ? id.Value.ToString() : "null";
            var perm = Norm(request.PermissionCode);

            return
                $"employees:list:" +
                $"c{companyId}:v{version}:" +
                $"me{employeeId}:r{roleLevel}:" +
                $"p{request.PageIndex}:s{request.PageSize}:" +
                $"sc{Norm(request.SortColumn)}:{Norm(request.SortDirection)}:" +
                $"q{Norm(request.searchKey)}:" +
                $"b{IdKey(request.BranchId)}:" +
                $"perm{perm}";
        }

        #endregion


        #region Areas list cache keys
        public static string AreasVersion(int companyId) => $"v:areas:{companyId}";

        public static string AreasList(int companyId, AreaRequest request, int version)
        {
            string Norm(string? s) => (s ?? "").Trim().ToLowerInvariant();

            return
                $"areas:list:" +
                $"c{companyId}:v{version}:" +
                $"p{request.PageIndex}:s{request.PageSize}:" +
                $"sc{Norm(request.SortColumn)}:{Norm(request.SortDirection)}:" +
                $"q{Norm(request.searchKey)}";
        }
        #endregion

        #region Branches list cache keys
        public static string BranchesVersion(int companyId) => $"v:branches:{companyId}";
        public static string BranchesList(int companyId, int employeeId, int roleLevel, BranchRequest request, int version)
        {
            string Norm(string? s) => (s ?? "").Trim().ToLowerInvariant();

            return
                $"branches:list:" +
                $"c{companyId}:v{version}:me{employeeId}:r{roleLevel}:" +
                $"p{request.PageIndex}:s{request.PageSize}:" +
                $"sc{Norm(request.SortColumn)}:{Norm(request.SortDirection)}:" +
                $"q{Norm(request.searchKey)}";
        }
        #endregion


        #region Employee dashboard cache keys

        public static string EmployeeDashboardVersion(int employeeId) => $"v:empdash:{employeeId}";
        public static string EmployeeDashboard(int companyId, int employeeId, string periodKey, int version)
            => $"dashboard:employee:{companyId}:v{version}:emp{employeeId}:{periodKey}";

        public static string EmployeeTasksWithoutCommentsToday(int companyId, int employeeId, int roleLevel, BaseApiRequest request, int version)
            => $"dashboard:emp:noCommentToday:{companyId}:v{version}:emp{employeeId}:role{roleLevel}:" +
               $"p{request.PageIndex}:s{request.PageSize}:q{request.searchKey}";

        public static string EmployeeWarnings(int companyId, int employeeId, string periodKey, int version)
            => $"dashboard:emp:warnings:{companyId}:v{version}:emp{employeeId}:{periodKey}";

        public static string EmployeeDeductions(int companyId, int employeeId, string periodKey, int version)
            => $"dashboard:emp:deductions:{companyId}:v{version}:emp{employeeId}:{periodKey}";

        public static string EmployeeDueSoonTasks(int companyId, int employeeId, int version)
            => $"dashboard:emp:dueSoon:{companyId}:v{version}:emp{employeeId}";

        public static string EmployeeKpisExtended(int companyId, int employeeId, string periodKey, int version)
            => $"dashboard:emp:kpisx:{companyId}:v{version}:emp{employeeId}:{periodKey}";

        public static string EmployeeCompletedTasksDetails(int companyId, int employeeId, string periodKey, int version)
            => $"dashboard:emp:completedDetails:{companyId}:v{version}:emp{employeeId}:{periodKey}";
        #endregion



        #region Departments cache keys (by branch)
        public static string DepartmentsVersion(int? branchId)
            => $"v:departments:branch:{branchId ?? 0}";
        public static string DepartmentsList(DepartmentRequest request, int version)
            => $"departments:list:v{version}:" +
               $"branch{request.BranchId ?? 0}:" +
               $"p{request.PageIndex}:s{request.PageSize}:" +
               $"sort{request.SortColumn}:{request.SortDirection}:" +
               $"q{request.searchKey}";

        #endregion


        #region Task comments cache keys

        public static string TaskCommentsVersion(int taskId)
            => $"v:taskcomments:{taskId}";

        public static string TaskCommentsList(int taskId, TaskCommentRequest request, int version)
            => $"taskcomments:list:task{taskId}:v{version}:" +
               $"p{request.PageIndex}:s{request.PageSize}:" +
               $"sort{request.SortColumn}:{request.SortDirection}:" +
               $"q{request.searchKey}";

        #endregion


        #region Task close requests cache keys
        public static string TaskCloseRequestsVersion(int taskId)
            => $"v:taskclosereq:{taskId}";

        public static string TaskCloseRequestsList(int taskId, TaskCloseRequestRequest request, int version)
            => $"taskclosereq:list:task{taskId}:v{version}:" +
               $"p{request.PageIndex}:s{request.PageSize}:" +
               $"sort{request.SortColumn}:{request.SortDirection}:" +
               $"q{request.searchKey}";

        #endregion

        #region Task discounts cache keys

        public static string TaskDiscountsVersion(int taskId)
            => $"v:taskdiscounts:{taskId}";

        public static string TaskDiscountsList(int taskId, TaskDiscountRequest request, int version)
            => $"taskdiscounts:list:task{taskId}:v{version}:" +
               $"p{request.PageIndex}:s{request.PageSize}:" +
               $"sort{request.SortColumn}:{request.SortDirection}:" +
               $"q{request.searchKey}";

        #endregion

        #region Task extension requests cache keys
        public static string TaskExtensionRequestsVersion(int taskId)
            => $"v:taskextreq:{taskId}";

        public static string TaskExtensionRequestsList(int taskId, TaskExtensionRequestRequest request, int version)
            => $"taskextreq:list:task{taskId}:v{version}:" +
               $"p{request.PageIndex}:s{request.PageSize}:" +
               $"sort{request.SortColumn}:{request.SortDirection}:" +
               $"q{request.searchKey}";

        #endregion


        #region Task warnings cache keys
        public static string TaskWarningsVersion(int taskId)
            => $"v:taskwarnings:{taskId}";

        public static string TaskWarningsList(int taskId, WarningRequest request, int version)
            => $"taskwarnings:list:task{taskId}:v{version}:" +
               $"p{request.PageIndex}:s{request.PageSize}:" +
               $"sort{request.SortColumn}:{request.SortDirection}:" +
               $"q{request.searchKey}";

        #endregion


        #region Leaves cache keys

        public static string LeavesVersion(int companyId)
            => $"v:leaves:{companyId}";

        public static string LeavesList(
            int companyId,
            int roleLevel,
            int employeeId,
            LeaveRequest request,
            int version)
        {
            var status = request.StatusId ?? 0;
            var empIdsKey = (request.EmployeeIds == null || !request.EmployeeIds.Any())
                ? "all"
                : string.Join("-", request.EmployeeIds.Distinct().OrderBy(x => x));

            return $"leaves:list:{companyId}:v{version}:" +
                   $"role{roleLevel}:viewer{employeeId}:" +
                   $"status{status}:emps{empIdsKey}:" +
                   $"p{request.PageIndex}:s{request.PageSize}:" +
                   $"sort{request.SortColumn ?? "CreatedDate"}:{request.SortDirection ?? "DESC"}:" +
                   $"q{request.searchKey}";
        }

        #endregion

        #region Roles cache keys
        public static string RolesVersion(int companyId)=> $"v:roles:{companyId}";

        public static string Roles(
            int companyId,
            BaseApiRequest request,
            int version)
            => $"roles:{companyId}:v{version}:" +
               $"p{request.PageIndex}:s{request.PageSize}:" +
               $"sort{request.SortColumn}:{request.SortDirection}:" +
               $"q{request.searchKey}";
        #endregion




       #region Employee roles cache keys
        public static string RoleAssignmentsVersion(int roleId)
            => $"v:roleAssign:{roleId}";

        public static string RoleAssignments(int roleId, RoleAssignmentReguest request, int version)
            => $"roleAssign:{roleId}:v{version}:" +
               $"p{request.PageIndex}:s{request.PageSize}:" +
               $"sort{request.SortColumn}:{request.SortDirection}:" +
               $"q{request.searchKey}:" +
               $"assigned{request.IsAssigned}";

        #endregion
    }


    }
