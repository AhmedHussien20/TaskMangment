import { Injectable } from '@angular/core';
import { BranchRepository } from '../repositories/branch.repository';
import { SearchCriteria } from '../models/search-criteria.model';
import { map, Observable } from 'rxjs';
import { HttpParams } from '@angular/common/http';
import { BranchModel } from '../models/branch/branch-model';

@Injectable({
  providedIn: 'root'
})
export class BranchService {

  constructor(private branchRepository: BranchRepository) { }

  fetchBranchs(searchCriteria: SearchCriteria, entries: number): Observable<any> {
    // Create a copy of the searchCriteria object to avoid modifying the original
    const { filterTypes, ...filter } = searchCriteria;
    const criteria = { ...filter, pageCount: entries };
    let params = new HttpParams();
    Object.keys(filter).forEach(key => {
      if (filter[key] !== null && filter[key] !== undefined && filter[key] !== '' && filter[key] !== 'filterTypes') {
        params = params.append(key, filter[key]);
      }
    });
    return this.branchRepository.getAll(params).pipe(
      map(response => {
        // Process the response if needed
        return {
          data: response.data as BranchModel[],
          totalItems: response.totalItems,
          targetPage: response.targetPage,
          itemCount: response.itemCount
        };
      })
    );
  }



}

