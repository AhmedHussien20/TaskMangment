import { Injectable } from '@angular/core';
import { ApiService } from 'app/core/services/api.service';
import { HttpClient } from '@angular/common/http'; 
import { AreaRequest } from '../models/area/area';

@Injectable({ providedIn: 'root' })
export class AreaRepository {

  private api: ApiService;

  constructor(private http: HttpClient) {
    this.api = new ApiService(http);
    this.api.serviceName = 'Area';
  }

getAll(request: AreaRequest) {
  const query = this.buildQuery(request);
  return this.api.get(`?${query}`);
}

private buildQuery(req: AreaRequest): string {
  const params = [];

  if (req.name) params.push(`Name=${encodeURIComponent(req.name)}`);
  if (req.companyId) params.push(`CompanyId=${req.companyId}`);

  params.push(`PageIndex=${req.pageIndex}`);
  params.push(`PageSize=${req.pageSize}`);
  params.push(`SortColumn=${req.sortColumn}`);
  params.push(`SortDirection=${req.sortDirection}`);

  return params.join('&');
}


  getById(id: number) {
    return this.api.get(`/${id}`);
  }

  create(model: any) {
    return this.api.post('', model);
  }

  update(id: number, model: any) {
    return this.api.put(`/${id}`, model);
  }

  delete(id: number) {
    return this.api.delete(`/${id}`);
  }
}
