import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from 'app/core/services/api.service';
import { BaseResponse } from 'app/models/base.response.model';
import { EmpTasksChartResultDto } from '../models/chart';

@Injectable({
  providedIn: 'root'
})
export class ChartService {

  private readonly service = 'Chart'; 

  constructor(private api: ApiService) {}

  getEmployeeTasksChart(employeeId: number, fromDate: string, toDate?: string)
    : Observable<BaseResponse<EmpTasksChartResultDto>> {    
    const query = [
      `employeeId=${employeeId}`,
      `from=${encodeURIComponent(fromDate)}`,
      toDate ? `to=${encodeURIComponent(toDate)}` : ''
    ].filter(Boolean).join('&');

    return this.api.get<BaseResponse<EmpTasksChartResultDto>>(this.service, `emp-tasks-chart?${query}`);
  }
}
