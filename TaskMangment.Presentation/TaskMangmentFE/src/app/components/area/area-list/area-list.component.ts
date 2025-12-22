import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule } from '@angular/forms';
import { NgbModal, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';

import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { GenericTableComponent } from '../../../shared/components/generic-table/generic-table.component';
import { Area, AreaRequest } from 'app/core/models/area/area';
import { AreaService } from 'app/core/services/area.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { AreaCreateUpdateComponent } from '../area-create-update.component/area-create-update.component.component';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';


declare var bootstrap: any;

@Component({
  selector: 'app-area-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule,
    AreaCreateUpdateComponent
  ],
  templateUrl: './area-list.component.html',
  styleUrls: ['./area-list.component.scss']
})

export class AreaListComponent implements OnInit {

  title = 'AREA.LIST_TITLE';
  breadcrumbs = ['HOME', 'AREAS'];
  activeitem = 'AREA.LIST_TITLE';

  // Table Columns
  columns = [
    { key: 'id', label: 'AREA.ID' },
    { key: 'name', label: 'AREA.NAME' },
    { key: 'address', label: 'AREA.ADDRESS' },
    { key: 'managerName', label: 'AREA.MANAGER' },
    { key: 'branchCount', label: 'AREA.BRANCHES' }
  ];

  // DATA
  rows: Area[] = [];
  totalItems = 0;

  // Pagination
  page = 1;
  entries = 10;

  // Filters Object SAME as expiry style
  searchCriteria: SearchCriteria = {

    searchKey: '',
    pageIndex: this.page,
    pageSize: this.entries,

    sortColumn: 'Id',
    sortDirection: 'ASC',

    filterTypes: {
      searchKey: 'text',
    },

  };

  isLoading = false;
  labels = {
    searchKey: 'AREA.searchKey',
  }
  formUrl = '/areas/add';

  constructor(
    private router: Router,
    private areaService: AreaService,
    private modalService: NgbModal, private fb: FormBuilder,
    private translate: TranslateService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  navigateToForm(): void {
    this.router.navigate(['/area/add']);
  }

  //  Load Data from Service
  loadData() {
    this.isLoading = true;

    this.areaService.getAll(this.searchCriteria).subscribe({
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

  //  Edit handler
  onEdit(id: number) {
    this.router.navigate(['/areas/edit', id]);
  }

  onDelete(id: number) {
    if (!confirm('Delete this area?')) return;

    this.areaService.delete(id).subscribe(() => this.loadData());
  }
  modal: any;
  selectedAreaId: number | null = null;
  isEdit = false;

  openAdd(modal: any) {
    this.isEdit = false;
    this.selectedAreaId = null;
    this.open(modal);
  }

  openEdit(id: number, modal: any) {
    this.isEdit = true;
    this.selectedAreaId = id;
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

confirmDelete(areaId: number) {
  Swal.fire({
    title: this.translate.instant('COMMON.CONFIRM_DELETE_TITLE'),
    text: this.translate.instant('COMMON.CONFIRM_DELETE_TEXT'),
    icon: 'warning',
    showCancelButton: true,
    confirmButtonText: this.translate.instant('COMMON.DELETE_BUTTON'),
    cancelButtonText: this.translate.instant('COMMON.CANCEL_BUTTON'),
    confirmButtonColor: '#d33',
    cancelButtonColor: '#6c757d'
  }).then((result) => {
    if (result.isConfirmed) {
      this.deleteTask(areaId);
    }
  });
}

deleteTask(areaId: number) {
  this.isLoading = true;

  this.areaService.delete(areaId).subscribe({
    next: () => {
    this.toastr.success(this.translate.instant('COMMON.DELETE_SUCCESS'));


      this.isLoading = false;
      this.loadData();
    },
    error: () => {
      this.isLoading = false;
    }
  });
}


}
