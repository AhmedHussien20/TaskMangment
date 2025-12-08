// app/core/repositories/branch-availability.repository.ts

import { Injectable } from '@angular/core';
import { map, Observable } from 'rxjs';
import { ApiService } from '../services/api.service';
import { ModuleAvailability } from '../models/branch/branch-availability.model';
import { BaseResponse } from 'app/models/base.response.model';

@Injectable({
  providedIn: 'root',
})
export class BranchAvailabilityRepository {
  constructor(private apiService: ApiService) {}


  getBranchAvailability(
    branchId: number
  ): Observable<{ branchCode: number; modules: ModuleAvailability[] }> {
    return this.apiService
      .get<
        BaseResponse<{
          branchCode: number;
          modules: ModuleAvailability[];
        }>
      >(`v1.0/admin/BranchAvailability/${branchId}`)
      .pipe(map((response) => response.data));
  }

  
  updateModuleAvailability(
    branchId: number, 
    module: ModuleAvailability
  ): Observable<ModuleAvailability> {
    return this.apiService
      .put<BaseResponse<any>>(
        'v1.0/admin/BranchAvailability',
        {
          branchId,
          moduleId: module.id,
          activeFlag: !module.activeFlag,
        }
      )
      .pipe(map((response) => response.data));
  }
}
