// dashboard.service.ts
import { Injectable } from '@angular/core';
import { ApiService } from 'app/core/services/api.service';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { AdminDashboardDto, CompletedTasksTodayDto, EmployeeDashboardDto, InProgressUpdatedTodayDto, PendingCloseRequestDto, TaskStatusDto } from '../models/dashboard/dashboard.model';

@Injectable({
    providedIn: 'root'
})
export class DashboardService {

    private readonly service = 'dashboard';

    constructor(private api: ApiService) { }

    getAdminDashboard(): Observable<BaseResponse<AdminDashboardDto>> {
        return this.api.get<BaseResponse<AdminDashboardDto>>(this.service, 'admin');
    }

    getEmployeeDashboard(): Observable<BaseResponse<EmployeeDashboardDto>> {
        return this.api.get<BaseResponse<EmployeeDashboardDto>>(this.service, 'employee');
    }

    getAdminInProgressUpdatedToday(): Observable<BaseResponse<InProgressUpdatedTodayDto[]>> {
        return this.api.get<BaseResponse<InProgressUpdatedTodayDto[]>>(this.service,'admin/in-progress-updated-today'); 
    }

    getAdminTasksByStatus(status: 'Active' | 'Overdue' | 'Active'): Observable<BaseResponse<TaskStatusDto[]>> {
      return this.api.get<BaseResponse<TaskStatusDto[]>>(this.service,`admin/tasks-by-status?status=${status}`);
    }


    // completed-tasks-today
getAdminCompletedTasksToday()
  : Observable<BaseResponse<CompletedTasksTodayDto[]>> {

  return this.api.get<BaseResponse<CompletedTasksTodayDto[]>>(
    this.service,
    'admin/completed-tasks-today'
  );
}

// pending-close-requests
getAdminPendingCloseRequests()
  : Observable<BaseResponse<PendingCloseRequestDto[]>> {

  return this.api.get<BaseResponse<PendingCloseRequestDto[]>>(
    this.service,
    'admin/pending-close-requests'
  );
}


}
