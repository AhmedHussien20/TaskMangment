import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
import { ApiService } from 'app/core/services/api.service';
import { SearchCriteria } from '../models/search-criteria.model';
import { Course, CourseAddEdit, CoursePagedResponse } from '../models/course/course';

@Injectable({
  providedIn: 'root'
})
export class CourseService {

  private readonly service = 'Course';

  constructor(private api: ApiService) {}

  // GET /Course?query
  getAll(request: any): Observable<any> {
    const query = this.buildQuery(request);
    return this.api.get<any>(this.service, `?${query}`);
  }

  // GET /Course/{id}
  getById(id: number): Observable<BaseResponse<CourseAddEdit>> {
    return this.api.get<BaseResponse<CourseAddEdit>>(this.service, `${id}`);
  }

  // GET /Course/{courseId}/subjects
getSubjectsByCourse(courseId: number): Observable<BaseResponse<{ id: number; title: string }[]>> {
  return this.api.get<BaseResponse<{ id: number; title: string }[]>>(this.service, `${courseId}/subjects`);
}


  // POST /Course
  create(model: CourseAddEdit): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  // PUT /Course/{id}
  update(id: number, model: CourseAddEdit): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

  // DELETE /Course/{id}
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