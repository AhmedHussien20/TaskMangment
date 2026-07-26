import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { ApiService } from 'app/core/services/api.service';
import { SearchCriteria } from '../models/search-criteria.model';
import { Employee, EmployeeAddEdit, EmployeePagedResponse, EnumItemDto } from '../models/employee/employee';
import {
  Employee360AccessDto,
  Employee360Dto,
  Employee360EmailItem,
  Employee360LeaveDto,
  Employee360NotificationItem,
  Employee360PagedRequest,
  Employee360PerformanceDto,
  EmployeeDeductionDto,
  EmployeeTimelineItem,
  EmployeeWarningDto
} from '../models/employee/employee-360';

@Injectable({
  providedIn: 'root'
})
export class EmployeeService {

  private readonly service = 'Employee';

  constructor(private api: ApiService) {}

  // GET /Employee?query
  getAll(request: any): Observable<any> {
    const query = this.buildQuery(request);
    return this.api.get<any>(this.service, `?${query}`);
  }

  // GET /Employee/{id}
  getById(id: number): Observable<BaseResponse<EmployeeAddEdit>> {
    return this.api.get<BaseResponse<EmployeeAddEdit>>(this.service, `${id}`);
  }

  get360(id: number, range: { from?: string; to?: string } = {}): Observable<BaseResponse<Employee360Dto>> {
    const params: string[] = [];
    if (range.from) params.push(`From=${encodeURIComponent(range.from)}`);
    if (range.to) params.push(`To=${encodeURIComponent(range.to)}`);
    const query = params.length ? `?${params.join('&')}` : '';
    return this.api.get<BaseResponse<Employee360Dto>>(this.service, `${id}/360${query}`);
  }

  getTimeline(id: number, request: {
    from?: string;
    to?: string;
    searchKey?: string;
    pageIndex?: number;
    pageSize?: number;
  } = {}): Observable<BaseResponse<{
    data: EmployeeTimelineItem[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
  }>> {
    const params = [
      `PageIndex=${request.pageIndex ?? 1}`,
      `PageSize=${request.pageSize ?? 20}`
    ];
    if (request.from) params.push(`From=${encodeURIComponent(request.from)}`);
    if (request.to) params.push(`To=${encodeURIComponent(request.to)}`);
    if (request.searchKey) params.push(`SearchKey=${encodeURIComponent(request.searchKey)}`);
    return this.api.get(this.service, `${id}/timeline?${params.join('&')}`);
  }

  getWarnings(id: number): Observable<BaseResponse<EmployeeWarningDto[]>> {
    return this.api.get<BaseResponse<EmployeeWarningDto[]>>(this.service, `${id}/warnings`);
  }

  getDeductions(id: number): Observable<BaseResponse<EmployeeDeductionDto[]>> {
    return this.api.get<BaseResponse<EmployeeDeductionDto[]>>(this.service, `${id}/deductions`);
  }

  getAccess(id: number): Observable<BaseResponse<Employee360AccessDto>> {
    return this.api.get(this.service, `${id}/access`);
  }

  getPerformance(id: number, range: { from?: string; to?: string } = {}): Observable<BaseResponse<Employee360PerformanceDto>> {
    const params: string[] = [];
    if (range.from) params.push(`From=${encodeURIComponent(range.from)}`);
    if (range.to) params.push(`To=${encodeURIComponent(range.to)}`);
    const query = params.length ? `?${params.join('&')}` : '';
    return this.api.get(this.service, `${id}/performance${query}`);
  }

  getLeave360(id: number, range: { from?: string; to?: string } = {}): Observable<BaseResponse<Employee360LeaveDto>> {
    const params: string[] = [];
    if (range.from) params.push(`From=${encodeURIComponent(range.from)}`);
    if (range.to) params.push(`To=${encodeURIComponent(range.to)}`);
    const query = params.length ? `?${params.join('&')}` : '';
    return this.api.get(this.service, `${id}/leave${query}`);
  }

  getEmails(id: number, request: Employee360PagedRequest = {}): Observable<BaseResponse<{
    data: Employee360EmailItem[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
  }>> {
    return this.api.get(this.service, `${id}/emails?${this.build360Query(request)}`);
  }

  getNotifications360(id: number, request: Employee360PagedRequest = {}): Observable<BaseResponse<{
    data: Employee360NotificationItem[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
  }>> {
    return this.api.get(this.service, `${id}/notifications?${this.build360Query(request)}`);
  }

  private build360Query(req: Employee360PagedRequest): string {
    const params = [
      `PageIndex=${req.pageIndex ?? 1}`,
      `PageSize=${req.pageSize ?? 20}`
    ];
    if (req.from) params.push(`From=${encodeURIComponent(req.from)}`);
    if (req.to) params.push(`To=${encodeURIComponent(req.to)}`);
    if (req.searchKey) params.push(`SearchKey=${encodeURIComponent(req.searchKey)}`);
    if (req.status) params.push(`Status=${encodeURIComponent(req.status)}`);
    if (req.type) params.push(`Type=${encodeURIComponent(req.type)}`);
    if (req.channel) params.push(`Channel=${encodeURIComponent(req.channel)}`);
    if (req.unreadOnly != null) params.push(`UnreadOnly=${req.unreadOnly}`);
    return params.join('&');
  }

  // POST /Employee
  create(model: FormData): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  // PUT /Employee/{id}
  update(id: number, model: FormData): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

  // DELETE /Employee/{id}
  delete(id: number): Observable<BaseResponse<any>> {
    return this.api.delete<BaseResponse<any>>(this.service, `${id}`);
  }

  // GET /Employee/function-codes
getFunctionCodes(): Observable<BaseResponse<EnumItemDto[]>> {
  return this.api.get<BaseResponse<EnumItemDto[]>>(this.service, 'function-codes');
}

exportExcel(request: SearchCriteria): Observable<Blob> {
  const query = this.buildQuery(request);
  return this.api.getBlob(this.service, `export/excel?${query}`);
}



  // Build Query String
  private buildQuery(req: SearchCriteria): string {
    return [
      `searchKey=${req.searchKey ?? ''}`,
      `PageIndex=${req.pageIndex}`,
      `PageSize=${req.pageSize}`,
      `SortColumn=${req.sortColumn}`,
      `SortDirection=${req.sortDirection}`,
      `roleLevel=${(req as any).roleLevel ?? ''}`,
      `permissionCode=${(req as any).permissionCode ?? ''}`,
      `branchId=${(req as any).branchId ?? ''}`,
      `isActive=${(req as any).isActive ?? ''}`


    ].join('&');
  }
}