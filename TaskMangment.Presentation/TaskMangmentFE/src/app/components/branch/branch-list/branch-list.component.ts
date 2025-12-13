import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';

import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { GenericTableComponent } from '../../../shared/components/generic-table/generic-table.component';
 
import { BranchService } from 'app/core/services/branch.service';

import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { TranslateModule } from '@ngx-translate/core';
import { BranchGetDto } from 'app/core/models/branch/branch';
import { BranchCreateUpdateComponent  } from '../branch-create-update.component/branch-create-update.component';

@Component({
  selector: 'app-branch-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule,
    BranchCreateUpdateComponent ,
    NgbModalModule
  ],
  templateUrl: './branch-list.component.html',
  styleUrls: ['./branch-list.component.scss']
})
export class BranchListComponent implements OnInit {

  title = 'BRANCH.LIST_TITLE';
  breadcrumbs = ['HOME', 'BRANCHES'];
  activeitem = 'BRANCH.LIST_TITLE';

  // table columns
  columns = [
    { key: 'id', label: 'BRANCH.ID' },
    { key: 'name', label: 'BRANCH.NAME' },
    { key: 'areaName', label: 'BRANCH.AREA' },
    { key: 'managerName', label: 'BRANCH.MANAGER' },
    { key: 'responsibleName', label: 'BRANCH.RESPONSIBLE' }
  ];

  rows: BranchGetDto[] = [];
  totalItems = 0;

  page = 1;
  entries = 10;

  searchCriteria: SearchCriteria = {
    searchKey: '',
    pageIndex: this.page,
    pageSize: this.entries,
    sortColumn: 'Id',
    sortDirection: 'ASC',
    filterTypes: {
      searchKey: 'text',
    }
  };

  labels = {
    searchKey: 'BRANCH.searchKey'
  };

  isLoading = false;

  selectedBranchId: number | null = null;
  isEdit = false;

  constructor(
    private branchService: BranchService,
    private modalService: NgbModal
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;

    this.branchService.getAll(this.searchCriteria).subscribe({
      next: (res: any) => {
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

key = 0;

openAdd(modal: any) {
  this.isEdit = false;
  this.selectedBranchId = null;
  this.key++; // force rebuild
  this.open(modal);
}

openEdit(id: number, modal: any) {
  this.isEdit = true;
  this.selectedBranchId = id;
  this.key++; 
  this.selectedBranchId = id;
  this.open(modal);
}


  open(content: any) {
    this.modalService.open(content, {
      centered: true,
      backdrop: true,
      size: 'lg',
      windowClass: 'effect-scale'
    });
  }

  onFormSubmitted() {
    this.modalService.dismissAll();
    this.loadData();
  }
}
