// dashboard.service.ts
import { Injectable } from '@angular/core';
import { ApiService } from 'app/core/services/api.service';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { AdminDashboardDto, AdminKpisExtendedDto, CompletedTasksTodayDto, DeductionDto, EmployeeDashboardDto, InProgressUpdatedTodayDto, MyTaskDto, PendingCloseRequestDto, TaskStatusDto, TodayCommentTaskDto, WarningDto } from '../models/dashboard/dashboard.model';
import { PeriodDto } from 'app/models/period-type.model';

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

  getEmployeeDashboard(): Observable<BaseResponse<EmployeeDashboardDto>> {
    return this.api.get<BaseResponse<EmployeeDashboardDto>>(this.service, 'employee');
  }

  getEmployeeWarnings(): Observable<BaseResponse<WarningDto[]>> {
  return this.api.get<BaseResponse<WarningDto[]>>(this.service, 'employee/warnings');
}

getEmployeeDeductions(): Observable<BaseResponse<DeductionDto[]>> {
  return this.api.get<BaseResponse<DeductionDto[]>>(this.service, 'employee/deductions');
}


  getTasksNotCommentToday(): Observable<BaseResponse<TodayCommentTaskDto[]>> { 
  return this.api.get<BaseResponse<TodayCommentTaskDto[]>>(this.service, 'not-comment-today');
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
