// dashboard.service.ts
import { Injectable } from '@angular/core';
import { ApiService } from 'app/core/services/api.service';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { AdminDashboardDto, AdminKpisExtendedDto, CompletedTasksTodayDto, PenalityDto, EmployeeDashboardDto, InProgressUpdatedTodayDto, MyTaskDto, PendingCloseRequestDto, TasksPagedResponse, TaskStatusDto, TodayCommentTaskDto, WarningDto, EmployeeKpisExtendedDto } from '../models/dashboard/dashboard.model';
import { PeriodDto } from 'app/models/period-type.model';
import { SearchCriteria } from '../models/search-criteria.model';
import { HttpParams } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class DashboardService {

  private readonly service = 'dashboard';

  constructor(private api: ApiService) { }

  getAdminDashboard(period: PeriodDto): Observable<BaseResponse<AdminDashboardDto>> {
    return this.api.get<BaseResponse<AdminDashboardDto>>(
      this.service,
      'admin',
      period as any
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
  let params = new HttpParams()
    .set('searchKey', request.searchKey ?? '')
    .set('PageIndex', request.pageIndex)
    .set('PageSize', request.pageSize)
    .set('SortColumn', request.sortColumn)
    .set('SortDirection', request.sortDirection);

  return this.api.get<BaseResponse<TasksPagedResponse>>(
    this.service,
    'not-comment-today',
    { params }
  );
}


getEmployeeKpis(period: PeriodDto) {
    return this.api.get<BaseResponse<EmployeeKpisExtendedDto>>(
      this.service,'employee/kpis',period);
  }

getEmployeeCompletedTasksDetails(period: PeriodDto) {
    return this.api.get<BaseResponse<any[]>>(this.service, 'employee/completed-tasks-details', period as any);
  }


  getAdminTasksByStatus(status: 'Active' | 'Overdue' | 'Active',period: PeriodDto): Observable<BaseResponse<TaskStatusDto[]>> {
    return this.api.get<BaseResponse<TaskStatusDto[]>>(this.service, `admin/tasks-by-status?status=${status}`,period as any);
  }


  getAdminInProgressUpdatedToday(period: PeriodDto) {
    return this.api.get<BaseResponse<InProgressUpdatedTodayDto[]>>(
      this.service,
      'admin/in-progress-updated-today',
      period as any
    );
  }

  getAdminCompletedTasksToday(period: PeriodDto) {
    return this.api.get<BaseResponse<CompletedTasksTodayDto[]>>(
      this.service,
      'admin/completed-tasks-today',
      period as any
    );
  }

  getAdminPendingCloseRequests(period: PeriodDto) {
    return this.api.get<BaseResponse<PendingCloseRequestDto[]>>(
      this.service,
      'admin/pending-close-requests',
      period as any
    );
  }

  getAdminKpis(period: PeriodDto) {
    return this.api.get<BaseResponse<AdminKpisExtendedDto>>(
      this.service,
      'admin/kpis',
      period
    );
  }

   getAdminDiscounts(period: PeriodDto) {
    return this.api.get<BaseResponse<any[]>>(this.service, 'admin/discounts', period as any);
  }

  getAdminHighPriorityTasks() {
    return this.api.get<BaseResponse<any[]>>(this.service, 'admin/high-priority-tasks');
  }

  getAdminCompletedTasksDetails(period: PeriodDto) {
    return this.api.get<BaseResponse<any[]>>(this.service, 'admin/completed-tasks-details', period as any);
  }
}
