import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { ApiService } from 'app/core/services/api.service';
import { SearchCriteria } from '../models/search-criteria.model';
import { Student, StudentAddEdit, StudentPagedResponse } from '../models/student/student';

@Injectable({
  providedIn: 'root'
})
export class StudentService {

  private readonly service = 'Student';

  constructor(private api: ApiService) {}

  // GET /Student?query
  getAll(request: any): Observable<any> {
    const query = this.buildQuery(request);
    return this.api.get<any>(this.service, `?${query}`);
  }

  // GET /Student/{id}
  getById(id: number): Observable<BaseResponse<StudentAddEdit>> {
    return this.api.get<BaseResponse<StudentAddEdit>>(this.service, `${id}`);
  }

  // POST /Student
  create(model: StudentAddEdit): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  // PUT /Student/{id}
  update(id: number, model: StudentAddEdit): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

  // DELETE /Student/{id}
  delete(id: number): Observable<BaseResponse<any>> {
    return this.api.delete<BaseResponse<any>>(this.service, `${id}`);
  }

  // Build Query String
  private buildQuery(req: SearchCriteria): string {
    return [
      `searchKey=${req.searchKey ?? ''}`,
      `PageIndex=${req.pageIndex}`,
      `PageSize=${req.pageSize}`,
      `SortColumn=${req.sortColumn}`,
      `SortDirection=${req.sortDirection}`
    ].join('&');
  }
}