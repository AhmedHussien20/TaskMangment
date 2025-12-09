import { Injectable } from '@angular/core';
import { ApiService } from 'app/core/services/api.service';
import { HttpClient } from '@angular/common/http'; 
import { AreaPagedResponse, AreaRequest } from '../models/area/area';

@Injectable({
  providedIn: 'root'
})
export class AreaService {

  constructor(private http: HttpClient, private api: ApiService) {
    this.api.serviceName = 'Area';
  }

getAll(request: any) {
  const query = this.buildQuery(request);
  return this.api.get<AreaPagedResponse>(`?${query}`);
}


  // GET /Area/{id}
  getById(id: number) {
    return this.api.get(`/${id}`);
  }

  // POST /Area
  create(model: any) {
    return this.api.post('', model);
  }

  // PUT /Area/{id}
  update(id: number, model: any) {
    return this.api.put(`/${id}`, model);
  }

  // DELETE /Area/{id}
  delete(id: number) {
    return this.api.delete(`/${id}`);
  }

  // Build the query string from AreaRequest
  private buildQuery(req: AreaRequest): string {
    const params: string[] = [];

    if (req.name) params.push(`Name=${encodeURIComponent(req.name)}`);
    if (req.companyId) params.push(`CompanyId=${req.companyId}`);

    params.push(`PageIndex=${req.pageIndex}`);
    params.push(`PageSize=${req.pageSize}`);
    params.push(`SortColumn=${req.sortColumn}`);
    params.push(`SortDirection=${req.sortDirection}`);

    return params.join('&');
  }
}
