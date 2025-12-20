import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';

import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { GenericTableComponent } from '../../../shared/components/generic-table/generic-table.component';

import { CourseService } from 'app/core/services/course.service';

import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Course } from 'app/core/models/course/course';
import { CourseCreateUpdateComponent } from '../course-create-update/course-create-update.component';
import { ToastrService } from 'ngx-toastr';
import Swal from 'sweetalert2';

@Component({
  selector: 'app-course-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule,
    CourseCreateUpdateComponent,
    NgbModalModule
  ],
  templateUrl: './course-list.component.html',
  styleUrls: ['./course-list.component.scss']
})
export class CourseListComponent implements OnInit {

  title = 'COURSE.LIST_TITLE';
  breadcrumbs = ['HOME', 'COURSES'];
  activeitem = 'COURSE.LIST_TITLE';

  // table columns
  columns = [
    { key: 'id', label: 'COURSE.ID' },
    { key: 'title', label: 'COURSE.TITLE' },
    { key: 'description', label: 'COURSE.DESCRIPTION' },
    { key: 'subjects', label: 'COURSE.SUBJECTS' },
    { key: 'offerCount', label: 'COURSE.OFFER_COUNT' }
  ];

  rows: Course[] = [];
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
    searchKey: 'COURSE.searchKey'
  };

  isLoading = false;

  selectedCourseId: number | null = null;
  isEdit = false;

  constructor(
    private courseService: CourseService,
    private modalService: NgbModal,
    private translate: TranslateService,
    private toastr: ToastrService
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;

    this.courseService.getAll(this.searchCriteria).subscribe({
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
    this.selectedCourseId = null;
    this.key++; // force rebuild
    this.open(modal);
  }

  openEdit(id: number, modal: any) {
    this.isEdit = true;
    this.selectedCourseId = id;
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
  confirmDelete(courseId: number) {
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
        this.deleteCourse(courseId);
      }
    });
  }
  
  deleteCourse(courseId: number) {
    this.isLoading = true;

    this.courseService.delete(courseId).subscribe({
      next: () => {
      this.toastr.success(this.translate.instant('COMMON.DELETE_SUCCESS'));
  
  
        this.isLoading = false;
        this.loadData();
      },
      error: () => {
        this.isLoading = false;
  
        this.toastr.error(
          this.translate.instant('COMMON.DELETE_FAILED'),
          undefined,
          { timeOut: 3000 }
        );
      }
    });
  }
  
}