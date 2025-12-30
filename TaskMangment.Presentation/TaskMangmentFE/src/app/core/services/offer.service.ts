import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { ApiService } from "./api.service";
import { BaseResponse } from "app/models/base.response.model";
import { OfferPagedResponse,OfferAddEdit, OfferAssignStudents, OfferGet } from "../models/course/offers-course";

@Injectable({ providedIn: 'root' })
export class OfferService {

  private readonly service = 'Offer';

  constructor(private api: ApiService) {}


  getAll(request: any): Observable<BaseResponse<OfferPagedResponse>> {
    const query = this.buildQuery(request);
    return this.api.get<BaseResponse<OfferPagedResponse>>(this.service, `?${query}`);
  }


  getById(id: number): Observable<BaseResponse<OfferGet>> {
    return this.api.get<BaseResponse<OfferGet>>(this.service, `${id}`);
  }


  create(model: OfferAddEdit): Observable<BaseResponse<OfferGet>> {
    return this.api.post<BaseResponse<OfferGet>>(this.service, '', model);
  }


  update(id: number, model: OfferAddEdit): Observable<BaseResponse<OfferGet>> {
    return this.api.put<BaseResponse<OfferGet>>(this.service, `${id}`, model);
  }


  delete(id: number): Observable<BaseResponse<boolean>> {
    return this.api.delete<BaseResponse<boolean>>(this.service, `${id}`);
  }


  assignToStudents(offerId: number,model: OfferAssignStudents): Observable<BaseResponse<boolean>> {
    return this.api.post<BaseResponse<boolean>>(
      this.service, `${offerId}/assign-students`, model);
  }


  private buildQuery(req: any): string {
    return [
      `searchKey=${req.searchKey || ''}`,
      `PageIndex=${req.pageIndex}`,
      `PageSize=${req.pageSize}`,
      `SortColumn=${req.sortColumn}`,
      `SortDirection=${req.sortDirection}`
    ].join('&');
  }
}
