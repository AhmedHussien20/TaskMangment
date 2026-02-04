// dashboard.service.ts
import { Injectable } from '@angular/core';
import { ApiService } from 'app/core/services/api.service';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { AdminDashboardDto, AdminKpisExtendedDto, CompletedTasksTodayDto, PenalityDto, EmployeeDashboardDto, InProgressUpdatedTodayDto, MyTaskDto, PendingCloseRequestDto, TasksPagedResponse, TaskStatusDto, TodayCommentTaskDto, WarningDto, EmployeeKpisExtendedDto, PagedResponse, BranchFilterDto } from '../models/dashboard/dashboard.model';
import { PeriodDto } from 'app/models/period-type.model';
import { SearchCriteria } from '../models/search-criteria.model';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  private readonly service = 'dashboard';

  constructor(private api: ApiService) { }

 getAdminDashboard(period: PeriodDto, branchId?: number): Observable<BaseResponse<AdminDashboardDto>> {
  const query: any = { ...(period as any) };

  if (branchId != null) {
    query.branchId = branchId;
  }

  return this.api.get<BaseResponse<AdminDashboardDto>>(
    this.service,
    'admin',
    query
  );
}

  getEmployeeDashboard(period: PeriodDto): Observable<BaseResponse<EmployeeDashboardDto>> {
    return this.api.get<BaseResponse<EmployeeDashboardDto>>(this.service, 'employee',period as any);
  }

  getEmployeeWarnings(period: PeriodDto): Observable<BaseResponse<WarningDto[]>> {
  return this.api.get<BaseResponse<WarningDto[]>>(this.service, 'employee/warnings',period as any);
}
getMyDueSoonTask(): Observable<BaseResponse<MyTaskDto[]>> {
  return this.api.get<BaseResponse<MyTaskDto[]>>(this.service, 'due-soon-tasks');
}

getEmployeePenalities(period: PeriodDto): Observable<BaseResponse<PenalityDto[]>> {
  return this.api.get<BaseResponse<PenalityDto[]>>(this.service, 'employee/deductions',period as any);
}

getTasksNotCommentToday(request: any): Observable<BaseResponse<TasksPagedResponse>> {

  const query: any = {
    searchKey: request.searchKey ?? '',
    PageIndex: request.pageIndex,
    PageSize: request.pageSize,
    SortColumn: request.sortColumn,
    SortDirection: request.sortDirection
  };

  return this.api.get<BaseResponse<TasksPagedResponse>>(
    this.service,
    'not-comment-today',
    query
  );
}



getEmployeeKpis(period: PeriodDto) {
    return this.api.get<BaseResponse<EmployeeKpisExtendedDto>>(
      this.service,'employee/kpis',period);
  }

getEmployeeCompletedTasksDetails(period: PeriodDto) {
    return this.api.get<BaseResponse<any[]>>(this.service, 'employee/completed-tasks-details', period as any);
  }


  getAdminTasksByStatus(
  status: 'Active' | 'Overdue' | 'Completed',
  period: PeriodDto,
  branchId?: number
): Observable<BaseResponse<TaskStatusDto[]>> {

  const query: any = { ...(period as any) };
  if (branchId != null) query.branchId = branchId;

  return this.api.get<BaseResponse<TaskStatusDto[]>>(
    this.service,
    `admin/tasks-by-status?status=${status}`,
    query
  );
}



 getAdminInProgressUpdatedToday(request: any, branchId?: number) {
  const query: any = {
    searchKey: request.searchKey ?? '',
    PageIndex: request.pageIndex,
    PageSize: request.pageSize,
    SortColumn: request.sortColumn ?? 'UpdatedAt',
    SortDirection: request.sortDirection ?? 'DESC',
    'Period.Type': request.period?.type
  };

  if (request.period?.from) query['Period.From'] = request.period.from;
  if (request.period?.to) query['Period.To'] = request.period.to;

  if (branchId != null) query.branchId = branchId;

  return this.api.get<BaseResponse<any>>(
    this.service,
    'admin/in-progress-updated-today',
    query
  );
}






getAdminCompletedTasksToday(period: PeriodDto, branchId?: number) {
  const query: any = { ...(period as any) };
  if (branchId != null) query.branchId = branchId;

  return this.api.get<BaseResponse<CompletedTasksTodayDto[]>>(
    this.service,
    'admin/completed-tasks-today',
    query
  );
}


  getAdminPendingCloseRequests(period: PeriodDto, branchId?: number) {
  const query: any = { ...(period as any) };
  if (branchId != null) query.branchId = branchId;

  return this.api.get<BaseResponse<PendingCloseRequestDto[]>>(
    this.service,
    'admin/pending-close-requests',
    query
  );
}


 getAdminKpis(period: PeriodDto, branchId?: number) {
  const query: any = { ...(period as any) };
  if (branchId != null) query.branchId = branchId;

  return this.api.get<BaseResponse<AdminKpisExtendedDto>>(
    this.service,
    'admin/kpis',
    query
  );
}


  getAdminDiscounts(period: PeriodDto, branchId?: number) {
  const query: any = { ...(period as any) };
  if (branchId != null) query.branchId = branchId;

  return this.api.get<BaseResponse<any[]>>(
    this.service,
    'admin/discounts',
    query
  );
}


  getAdminHighPriorityTasks(branchId?: number) {
  const query: any = {};
  if (branchId != null) query.branchId = branchId;

  return this.api.get<BaseResponse<any[]>>(
    this.service,
    'admin/high-priority-tasks',
    query
  );
}

  getAdminCompletedTasksDetails(period: PeriodDto, branchId?: number) {
  const query: any = { ...(period as any) };
  if (branchId != null) query.branchId = branchId;

  return this.api.get<BaseResponse<any[]>>(
    this.service,
    'admin/completed-tasks-details',
    query
  );
}


  getBranchesForFilter(): Observable<BaseResponse<BranchFilterDto[]>> {
  return this.api.get<BaseResponse<BranchFilterDto[]>>(
    this.service,
    'admin/branches-for-filter'
  );
}

}
