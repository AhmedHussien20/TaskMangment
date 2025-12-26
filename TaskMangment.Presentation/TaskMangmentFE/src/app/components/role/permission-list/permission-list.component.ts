import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule, NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';

import { PageHeaderComponent } from 'app/shared/components/page-header/page-header.component';
import { GenericTableComponent } from 'app/shared/components/generic-table/generic-table.component';

import { Permission } from 'app/core/models/roles/permissions';
import { PermissionService } from 'app/core/services/permission.service';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { PermissionCreateUpdateComponent } from '../permission-create-update/permission-create-update.component';

@Component({
selector: 'app-permission-list',
standalone: true,
imports: [
CommonModule,
FormsModule,
NgbPaginationModule,
TranslateModule,
PageHeaderComponent,
GenericTableComponent,
PermissionCreateUpdateComponent
],
templateUrl: './permission-list.component.html'
})
export class PermissionListComponent implements OnInit {

title = 'PERMISSION.LIST_TITLE';
activeitem = 'PERMISSION.LIST_TITLE';
breadcrumbs = [
  'MENU.HOME',
  'MENU.EMPLOYEES',
  'PERMISSION.LIST_TITLE'
];

isEdit = false;

columns = [
{ key: 'id', label: 'PERMISSION.ID' },
{ key: 'code', label: 'PERMISSION.CODE' },
{ key: 'name', label: 'PERMISSION.NAME' },
{ key: 'description', label: 'PERMISSION.DESCRIPTION' }
];

rows: Permission[] = [];
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
searchKey: 'text'
}
};

labels = {
searchKey: 'PERMISSION.SEARCH'
};

isLoading = false;
selectedPermissionId: number | null = null;

constructor(
private permissionService: PermissionService,
private modalService: NgbModal
) {}

ngOnInit(): void {
this.loadData();
}

loadData() {
this.isLoading = true;


this.permissionService.getAll(this.searchCriteria).subscribe({
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
this.selectedPermissionId = null;
this.open(modal);
}

openEdit(id: number, modal: any) {
this.isEdit = true;
this.selectedPermissionId = id;
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
