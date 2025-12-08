import { Injectable } from '@angular/core';
import { HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiService } from '../services/api.service';
import { BranchModel } from '../models/branch/branch-model';

@Injectable({
    providedIn: 'root',
})
export class BranchRepository {
    constructor(private apiService: ApiService) { }

    getAll(
        params: HttpParams
    ): Observable<{
        data: BranchModel[];
        totalItems: number;
        targetPage: number;
        itemCount: number;
    }> {
        return this.apiService.get<{
            data: BranchModel[];
            totalItems: number;
            targetPage: number;
            itemCount: number;
        }>(`v1.0/admin/Branch`, { params: params });
    }
}