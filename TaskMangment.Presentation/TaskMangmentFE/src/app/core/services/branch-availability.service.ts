// app/core/services/branch-availability.service.ts

import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ModuleAvailability } from '../models/branch/branch-availability.model';
import { BranchAvailabilityRepository } from '../repositories/branch-availability.repository';

@Injectable({
  providedIn: 'root',
})
export class BranchAvailabilityService {
  constructor(private repository: BranchAvailabilityRepository) {}


  getBranchAvailability(
    branchId: number
  ): Observable<{
    branchCode: number;
    modules: ModuleAvailability[];
  }> {
    return this.repository.getBranchAvailability(branchId);
  }

  
  updateModuleAvailability(
    branchId: number,module: ModuleAvailability
  ): Observable<any> {
    return this.repository.updateModuleAvailability(branchId, module);
  }
}
