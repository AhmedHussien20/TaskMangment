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
export class JobService {

  private readonly service = 'Job';

  constructor(private api: ApiService) {}

  // GET /Job?query
  getAll(request: any): Observable<BaseResponse<any>> {
    const query = this.buildQuery(request);
    return this.api.get<BaseResponse<any>>(this.service, `?${query}`);
  }

  // GET /Job/{id}
  getById(id: number): Observable<BaseResponse<any>> {
    return this.api.get<BaseResponse<any>>(this.service, `${id}`);
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
