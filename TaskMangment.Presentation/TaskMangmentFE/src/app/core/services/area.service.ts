import { Injectable } from '@angular/core';
import { ApiService } from 'app/core/services/api.service';
import { HttpClient } from '@angular/common/http'; 
import { Area, AreaPagedResponse } from '../models/area/area';
import { SearchCriteria } from '../models/search-criteria.model';
import { Observable } from 'rxjs';
import { BaseResponse } from 'app/models/base.response.model';
@Injectable({
  providedIn: 'root'
})
export class AreaService {

  private readonly service = 'Area';

  constructor(private api: ApiService) {}

  // GET /Area?query
  getAll(request: any): Observable<BaseResponse<AreaPagedResponse>> {
    const query = this.buildQuery(request);
    return this.api.get<BaseResponse<AreaPagedResponse>>(this.service, `?${query}`);
  }

  // GET /Area/{id}
  getById(id: number): Observable<BaseResponse<Area>> {
    return this.api.get<BaseResponse<Area>>(this.service, `${id}`);
  }

  // POST /Area
  create(model: any): Observable<BaseResponse<any>> {
    return this.api.post<BaseResponse<any>>(this.service, '', model);
  }

  // PUT /Area/{id}
  update(id: number, model: any): Observable<BaseResponse<any>> {
    return this.api.put<BaseResponse<any>>(this.service, `${id}`, model);
  }

  // DELETE /Area/{id}
  delete(id: number): Observable<BaseResponse<any>> {
    return this.api.delete<BaseResponse<any>>(this.service, `${id}`);
  }

  // Build query string
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
