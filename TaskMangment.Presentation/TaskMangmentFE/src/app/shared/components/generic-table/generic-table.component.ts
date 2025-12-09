import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { Router } from '@angular/router';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { TranslateModule } from '@ngx-translate/core';

@Component({
  selector: 'app-generic-table',
  standalone: true,
  imports: [CommonModule, FormsModule, NgbPaginationModule,TranslateModule],
  templateUrl: './generic-table.component.html',
  styleUrls: ['./generic-table.component.scss']
})
export class GenericTableComponent<T = any> {
  @Input() title: string = '';  
  @Input() breadcrumbs: string[] = [];  
  @Input() activeitem: string = '';  
  @Input() columns: { 
    key: string, 
    label: string,
    isButton?: boolean,       
    icon?: string,
    onClick?: (item: any) => void 
  }[] = [];  
  @Input() data: any[] = [];  
  @Input() entries: number = 10;  
  @Input() page: number = 1;  
  @Input() totalItems: number = 0;  
  @Input() totalPages: number = 0;  
  @Input() showFilters: boolean = true;  
  @Input() showPagination: boolean = true;  
  @Input() formUrl: string = '';  
  @Input() searchCriteria: SearchCriteria<T> = new SearchCriteria<T>();
  @Input() showEditButton: boolean = false;
  @Input() showDeleteButton: boolean = false;
  @Input() showAddButton: boolean = false;
  @Input() statusOptions: { id: any, name: string }[] = [];
  @Input() isAssignedOptions: { id: any, name: string }[] = [];
  @Input() activeOptions: { id: any, name: string }[] = [];
  @Input() labels: { [key: string]: string } = {};
  @Input() onSearch?: (criteria: SearchCriteria<T>) => void; 
  @Input() addButtonLabel: string = 'Add New';
  @Input() onAddClick?: () => void;
  @Input() showCheckbox: boolean = false;
  @Input() loading: boolean = false; // Input property to control the spinner visibility
  @Input() hiddenFilters: string[] = []; // New input to specify hidden filters

  @Output() searchEvent = new EventEmitter<SearchCriteria<T>>();
  @Output() pageChange = new EventEmitter<number>();  
  @Output() entriesChange = new EventEmitter<number>();  
  @Output() editRow = new EventEmitter<any>();  
  @Output() selectedItemsChange = new EventEmitter<any[]>();
 
  initialSearchCriteria: SearchCriteria<T>;  
  selectedItems: any[] = [];

  constructor(private router: Router) {
    this.initialSearchCriteria = { ...this.searchCriteria };  
  }

  ngOnInit(): void {
    
    this.entries = this.searchCriteria.pageSize;
    this.loading = false;
  }

  navigateToForm() {
    this.router.navigate([this.formUrl]);
  }

  editItem(id: any) {
    this.router.navigate([this.formUrl, id, 'edit']); 
  }

  deleteItem(id: any) {
    console.log(`Delete item with ID: ${id}`);
  }

  applyFilters() {
    if (this.onSearch) {
      this.onSearch(this.searchCriteria);  
    } else {
      this.searchEvent.emit(this.searchCriteria);
    }
  }

clearFilters() {
  const ignore = ['sortColumn', 'sortDirection', 'pageIndex', 'pageSize'];

  Object.keys(this.searchCriteria).forEach(key => {

    // Ignore system fields
    if (ignore.includes(key)) return;

    const type = this.searchCriteria.filterTypes?.[key as keyof typeof this.searchCriteria];

    if (type === 'text') {
      this.searchCriteria[key] = '';
    } 
    else if (type === 'dropdown' || type === 'radio') {
      this.searchCriteria[key] = 0;
    } 
    else if (type === 'date') {
      this.searchCriteria[key] = null;
    }
  });

  this.applyFilters();
}


  
  toggleSelectAll(event: Event): void {
    const isChecked = (event.target as HTMLInputElement).checked;
    this.paginatedData.forEach(item => item.selected = isChecked);
    this.updateSelectedItems();
  }

  updateSelectedItems(): void {
    this.selectedItems = this.data.filter(item => item.selected); // Update selected items
    this.selectedItemsChange.emit(this.selectedItems); // Emit the selected items array
    console.log('Selected items:', this.selectedItems);
  }

  objectKeys(obj: any): string[] {
    // Exclude hidden filters from being displayed
    return Object.keys(obj).filter(
      key => key !== 'pageSize' && key !== 'pageIndex' && key !== 'filterTypes' && !this.hiddenFilters.includes(key)
    );
  }

  getInputType(key: string): string {
    return this.searchCriteria.filterTypes?.[key as keyof T] || 'text';
  }

  getDropdownOptions(key: string): any[] {
    if (key === 'statusId' && this.statusOptions) {
      return [{ id: 0, name: 'Select Status' }, ...this.statusOptions];  
    }
    if (key === 'isAssigned' && this.isAssignedOptions) {
      return [{ id: null, name: 'Select Assignment' }, ...this.isAssignedOptions];  
    }
    if (key === 'active' && this.activeOptions) {
      return [{ id: null, name: 'Select Active Status' }, ...this.activeOptions];  
    }
    return [];
  }

  getLabel(key: string): string {
    return this.labels[key] || key;
  }

  getStatusClass(status: string): string {
    switch (status) {
      case 'Saved':
        return 'tag tag-default';
      case 'Pending':
        return 'tag tag-warning';
      case 'Received':
        return 'tag tag-info';
      case 'Rejected':
        return 'tag tag-danger';
      case 'Confirmed':
        return 'tag tag-success';
      default:
        return 'tag tag-dark';
    }
  }

  // Pagination logic
  get paginatedData(): any[] {
    if (!Array.isArray(this.data)) {
      return [];
    }
    const startIndex = (this.page - 1) * this.entries;
    return this.data.slice(startIndex, startIndex + this.entries);
  }

  onPageChange(page: number) {
    this.page = page;
    this.pageChange.emit(page);  
  }
  
  onEntriesChange() {
    this.page = 1; // Reset to the first page when entries change
    this.entriesChange.emit(this.entries);  
  }

  onEdit(item: any): void {
    this.editRow.emit(item);
  }
  
  columnClick(col: { onClick?: (item: any) => void }, item: any): void {
    if (col.onClick) {
      col.onClick(item);
    }
  }  
}