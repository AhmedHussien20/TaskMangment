import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbModal, NgbModalModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';

import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';
import { GenericTableComponent } from '../../../shared/components/generic-table/generic-table.component';

import { StudentService } from 'app/core/services/student.service';

import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { TranslateModule } from '@ngx-translate/core';
import { Student } from 'app/core/models/student/student';
import { StudentCreateUpdateComponent } from '../student-create-update/student-create-update.component';

@Component({
  selector: 'app-student-list',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule,
    PageHeaderComponent,
    GenericTableComponent,
    TranslateModule,
    StudentCreateUpdateComponent,
    NgbModalModule
  ],
  templateUrl: './student-list.component.html',
  styleUrls: ['./student-list.component.scss']
})
export class StudentListComponent implements OnInit {

  title = 'STUDENT.LIST_TITLE';
  breadcrumbs = ['HOME', 'STUDENTS'];
  activeitem = 'STUDENT.LIST_TITLE';

  // table columns
  columns = [
    { key: 'id', label: 'STUDENT.ID' },
    { key: 'fullName', label: 'STUDENT.NAME' },
    { key: 'email', label: 'STUDENT.EMAIL' },
    { key: 'mobile', label: 'STUDENT.MOBILE' },
    { key: 'offerCount', label: 'STUDENT.OFFER_COUNT' }
  ];

  rows: Student[] = [];
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
    searchKey: 'STUDENT.searchKey'
  };

  isLoading = false;

  selectedStudentId: number | null = null;
  isEdit = false;

  constructor(
    private studentService: StudentService,
    private modalService: NgbModal
  ) { }

  ngOnInit(): void {
    this.loadData();
  }

  loadData() {
    this.isLoading = true;

    this.studentService.getAll(this.searchCriteria).subscribe({
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
    this.selectedStudentId = null;
    this.key++; // force rebuild
    this.open(modal);
  }

  openEdit(id: number, modal: any) {
    this.isEdit = true;
    this.selectedStudentId = id;
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
}