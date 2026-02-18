using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskMangment.Application.Common.ApiRequests.Task;

namespace TaskMangment.Infrastructure.Caching
{
    public static class CacheKeys
    {
        public static string TasksVersion(int companyId) => $"v:tasks:{companyId}";

        #region Dashboard cache keys
        public static string DashboardVersion(int companyId) => $"v:dashboard:{companyId}";

        public static string AdminDashboard(int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:admin:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";

        public static string AdminKpisExtended(int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:kpisx:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";

        public static string AdminDiscounts(int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:discounts:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";

        public static string AdminHighPriority(int companyId, int roleLevel, int? employeeId, int? branchId, int version)
            => $"dashboard:highprio:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}";
        public static string UpdatedTodayTasks(int companyId,int roleLevel,int? employeeId,int? branchId,string periodKey,int pageIndex,int pageSize,int version)
            => $"dashboard:todayupd:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}:p{pageIndex}:s{pageSize}";

        public static string CompletedTodayEmployees(int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:completedToday:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";

        public static string PendingCloseRequests(
            int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:pendingClose:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";

        public static string TasksByStatus(int companyId, string status, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:tasksByStatus:{companyId}:v{version}:status{status}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";

        public static string CompletedTasksDetails(int companyId, int roleLevel, int? employeeId, int? branchId, string periodKey, int version)
            => $"dashboard:completedDetails:{companyId}:v{version}:role{roleLevel}:emp{employeeId ?? 0}:branch{branchId ?? 0}:{periodKey}";



        #endregion


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
    }
}
