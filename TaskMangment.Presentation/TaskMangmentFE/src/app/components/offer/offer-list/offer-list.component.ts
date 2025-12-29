import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';

import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { GenericTableComponent } from '../../../shared/components/generic-table/generic-table.component';

import { OfferService } from 'app/core/services/offer.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { OfferCreateUpdateComponent } from '../offer-create-update/offer-create-update.component';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';
import { OfferGet } from 'app/core/models/course/offers-course';

@Component({
  selector: 'app-offer-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule,
    OfferCreateUpdateComponent,
    NgbModalModule
  ],
  templateUrl: './offer-list.component.html',
  styleUrls: ['./offer-list.component.scss']
})
export class OfferListComponent implements OnInit {

  title = 'OFFER.LIST_TITLE';
  activeitem = 'OFFER.LIST_TITLE';
  breadcrumbs = [
    'MENU.HOME',
    'MENU.EDUCATION',
    'OFFER.LIST_TITLE'
  ];

  // table columns
  columns = [
    { key: 'id', label: 'OFFER.ID' },
    { key: 'title', label: 'OFFER.TITLE' },
    { key: 'description', label: 'OFFER.DESCRIPTION' },
    { key: 'courseTitle', label: 'OFFER.COURSE' },
    { key: 'subjectTitle', label: 'OFFER.SUBJECT' },
    { key: 'assignedStudents', label: 'OFFER.ASSIGNED_STUDENTS' }
  ];

  rows: OfferGet[] = [];
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
    searchKey: 'OFFER.SEARCH'
  };

  isLoading = false;

  selectedOfferId: number | null = null;
  isEdit = false;
  key = 0;

  constructor(
    private offerService: OfferService,
    private modalService: NgbModal,
    private translate: TranslateService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;

    this.offerService.getAll(this.searchCriteria).subscribe({
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

  openAdd(modal: any) {
    this.isEdit = false;
    this.selectedOfferId = null;
    this.key++;
    this.open(modal);
  }

  openEdit(id: number, modal: any) {
    this.isEdit = true;
    this.selectedOfferId = id;
    this.key++;
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

  confirmDelete(offerId: number) {
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
        this.deleteOffer(offerId);
      }
    });
  }

  deleteOffer(offerId: number) {
    this.isLoading = true;

    this.offerService.delete(offerId).subscribe({
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
