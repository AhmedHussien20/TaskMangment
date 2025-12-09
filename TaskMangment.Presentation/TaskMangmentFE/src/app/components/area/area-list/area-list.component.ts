import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';

import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { GenericTableComponent } from '../../../shared/components/generic-table/generic-table.component';
import { Area, AreaRequest } from 'app/core/models/area/area';
import { AreaService } from 'app/core/services/area.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { TranslateModule } from '@ngx-translate/core';
 

@Component({
  selector: 'app-area-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule
  ],
  templateUrl: './area-list.component.html',
  styleUrls: ['./area-list.component.scss']
})
export class AreaListComponent implements OnInit {

  title = 'Area List';
  breadcrumbs = ['Home', 'Areas'];
  activeitem = 'Area List';

  // Table Columns
  columns = [
    { key: 'id', label: 'ID' },
    { key: 'name', label: 'Area Name' },
    { key: 'address', label: 'Address' },
    { key: 'managerName', label: 'Manager' },
    { key: 'branchCount', label: 'Branches' }
  ];

  // DATA
  rows: Area[] = [];
  totalItems = 0;

  // Pagination
  page = 1;
  entries = 10;

  // Filters Object SAME as expiry style
  searchCriteria: SearchCriteria={
  name: '',
  companyId: 0,

  pageIndex: this.page, 
  pageSize: this.entries,

  sortColumn: 'Id',
  sortDirection: 'ASC',

  filterTypes: {
    name: 'text',
    companyId: 'dropdown'
  }
};

  isLoading = false;

  formUrl = '/areas/add';

  constructor(
    private router: Router,
    private areaService: AreaService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  navigateToForm(): void {
    this.router.navigate(['/area/create']);
  }

  //  Load Data from Service
  loadData() {
    this.isLoading = true;

    this.areaService.getAll(this.searchCriteria).subscribe({
      next: (res:any) => {
        this.rows = res.data.data;
        this.totalItems = res.data.totalCount;
        this.page = res.data.pageIndex;
        this.entries = res.data.pageSize;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  onPageChange(page: number) {
    this.page = page;
    this.searchCriteria.pageIndex = page;
    this.loadData();
  }

  onEntriesChange(entries: number) {
    this.entries = entries;
    this.searchCriteria.pageSize = entries;
    this.searchCriteria.pageIndex = 1;
    this.page = 1;
    this.loadData();
  }

  applyFilters(filters: any) {
    this.searchCriteria = {
      ...this.searchCriteria,
      ...filters,
      pageIndex: 1
    };
    this.page = 1;
    this.loadData();
  }

  //  Edit handler
  onEdit(id: number) {
    this.router.navigate(['/areas/edit', id]);
  }

  onDelete(id: number) {
    if (!confirm('Delete this area?')) return;

    this.areaService.delete(id).subscribe(() => this.loadData());
  }
}
