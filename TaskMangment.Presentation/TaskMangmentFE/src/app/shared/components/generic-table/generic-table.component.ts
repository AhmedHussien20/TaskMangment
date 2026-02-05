import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule, NgbTooltipModule } from '@ng-bootstrap/ng-bootstrap';
import { NgSelectModule } from '@ng-select/ng-select';
import { TranslateModule } from '@ngx-translate/core';
import { MyDatePipe } from 'app/components/utilities/pipline/MyDatePipe';
import { SearchCriteria } from 'app/core/models/search-criteria.model';
import { Subject } from 'rxjs';
import { debounceTime } from 'rxjs/operators';


export type ColumnType =
  | 'text'
  | 'icon-action'
  | 'icon'
  | 'badge'
  | 'custom'
  | 'date'
  | 'dateTime'
  | 'assignees'
  | 'seen';

export interface BadgeConfig {
  text: string;
  class: string;
  icon?: string;
}

export interface TableColumn {
  key: string;
  label: string;
  type?: ColumnType;
  badgeMap?: Record<string, BadgeConfig>;
  icon?: string;
  displayField?: string;
}


interface HasId {
  id: any;
}
@Component({
  selector: 'app-generic-table',
  standalone: true,
  templateUrl: './generic-table.component.html',
  imports: [
    CommonModule,
    FormsModule,
    NgbPaginationModule, TranslateModule, MyDatePipe, NgbTooltipModule, NgSelectModule
  ],
  styleUrls: ['./generic-table.component.scss']
})
export class GenericTableComponent<T> implements OnDestroy{
  private filtersChanged$ = new Subject<void>();
  @Output() exportPdfClick = new EventEmitter<void>();
  @Output() exportExcelClick = new EventEmitter<void>();

  @Input() showExportPdf: boolean = false;
  @Input() showExportExcel: boolean = true;
  @Input() showExportReportExcel: boolean = false;

  @Input() formUrl: string = '';
  @Input() breadcrumbs: string[] = [];
  @Input() activeitem: string = '';
  @Input() showDetailsButton: boolean = false;

  @Output() details = new EventEmitter<number>();

  // ---------- Inputs ----------
  @Input() title: string = '';
  @Input() columns: TableColumn[] = [];
  @Input() data: T[] = [];
  @Input() entries: number = 10;
  @Input() page: number = 1;
  @Input() totalItems: number = 0;
  @Input() totalPages: number = 0;

  @Input() showFilters: boolean = true;
  @Input() showPagination: boolean = true;
  @Input() showCheckbox: boolean = false;
  @Input() showEditButton: boolean = false;
  @Input() showDeleteButton: boolean = false;

  @Input() showAddButton: boolean = false;
  @Input() addButtonLabel: string = '';
  @Input() disableActions: boolean = false;
  @Input() disableEditFn?: (row: T) => boolean;
@Input() disableDeleteFn?: (row: T) => boolean;
@Input() disableDetailsFn?: (row: T) => boolean;

  @Input() searchCriteria!: SearchCriteria<T>;
  @Input() labels: { [key: string]: string } = {};
  @Input() statusOptions: { id: any; name: string }[] = [];
  @Input() employeeOptions: { id: number; name: string }[] = [];
  // callback من الـ parent (زى Expiry)
  @Input() onSearch?: (criteria: SearchCriteria<T>) => void;
  @Input() onAddClick?: () => void;
  @Output() addClick = new EventEmitter<void>();
  @Input() checkboxKey?: string;

  // ---------- Outputs ----------
  @Output() pageChange = new EventEmitter<number>();
  @Output() entriesChange = new EventEmitter<number>();
  @Output() edit = new EventEmitter<number>();
  @Output() delete = new EventEmitter<number>();
  @Input() rowClickable: boolean = false;
  @Input() showEmployeeFilter: boolean = true;
  @Input() extraFilterOptions: Record<string, { value: any; label: string }[]> = {};

  // ---------- UI State ----------
  loading: boolean = false;             
  filtersOpen: boolean = true;

  sortColumn: string = '';
  sortDirection: 'ASC' | 'DESC' = 'ASC';

  // column resize state
  columnWidths: { [key: string]: string } = {};
  private resizingColumnKey: string | null = null;
  private resizeStartX: number = 0;
  private resizeStartWidth: number = 0;

  // ---------- Helpers ----------
  @Output() iconAction = new EventEmitter<{
    type: string;
    row: any;
  }>();

  @Output() checkboxChange = new EventEmitter<{
    row: any;
    checked: boolean;
  }>();


  /* ngOnInit(): void {
  this.filtersChanged$
    .pipe(debounceTime(400))
    .subscribe(() => {
      this.applyFilters(); 
    });
}
onFilterChange(key: string, value: any) {
  this.searchCriteria[key] = value;

  if (this.searchCriteria.pageIndex) this.searchCriteria.pageIndex = 1;

  this.filtersChanged$.next();
} */

  objectKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }

  getLabel(key: string): string {
    return this.labels?.[key] ?? key;
  }

  getBadgeText(col: TableColumn, item: any): string {
    if (!col.badgeMap) return '';
    const value = this.getValue(item, col.key);
    return col.badgeMap[value]?.text ?? '';
  }

  getBadgeClass(col: TableColumn, item: any): string {
    if (!col.badgeMap) return '';
    const value = this.getValue(item, col.key);
    return col.badgeMap[value]?.class ?? '';
  }

  getInitials(name: string): string {
    if (!name) return '';

    return name
      .split(' ')
      .map(x => x[0])
      .join('')
      .substring(0, 2)
      .toUpperCase();
  }
getExtraOptions(key: string) {
  return this.extraFilterOptions?.[key] ?? [];
}

  getInputType(key: string): 'text' | 'dropdown' | 'date' | 'dateTime' | 'number'|'toggle' {
    const filterTypes = (this.searchCriteria?.filterTypes || {}) as any;
    return filterTypes[key] || 'text';
  }

  getDropdownOptions(key: string) {
    switch (key) {
      case 'statusId':
        return this.statusOptions;

      case 'employeeIds':
        return this.employeeOptions;

case 'isAssigned':                 
      return this.statusOptions; 
      default:
        return [];
    }
  }

  // ---------- Filters ----------

  ngOnInit(): void {
    this.filtersChanged$
      .pipe(debounceTime(100))
      .subscribe(() => {
        (this.searchCriteria as any).pageIndex = 1;
        this.applyFilters();
      });
  }

   onFilterChange(key: string, value: any) {
    (this.searchCriteria as any)[key] = value;
    this.filtersChanged$.next();
  }
  applyFilters() {
    if (this.onSearch) {
      this.onSearch({ ...(this.searchCriteria || {}) });
    }
  }

  viewDetails(id: number) {
    this.details.emit(id);
  }

  isMultiSelect(key: string): boolean {
    if (key === 'employeeIds') return true;   // multi
    if (key === 'statusId') return false;     // single
    return false;
  }




  getItemId(item: T): any {
    return (item as any)['id'];
  }
  getValue(item: T, key: string): any {
    return (item as any)[key];
  }
  isItemSelected(item: T): boolean {
    return (item as any).selected || false;
  }

  setItemSelected(item: T, value: boolean): void {
    (item as any).selected = value;
  }
  clearFilters() {
    const ignore = ['sortColumn', 'sortDirection', 'pageIndex', 'pageSize'];

    const filterTypes = (this.searchCriteria?.filterTypes || {}) as Record<string, string>;

    Object.keys(filterTypes).forEach(key => {
      if (ignore.includes(key)) return;

      const type = filterTypes[key];

      if (type === 'text' || type === 'number') {
        (this.searchCriteria as any)[key] = '';
      } else if (type === 'dropdown' || type === 'radio') {
  if (this.isMultiSelect(key)) {
    (this.searchCriteria as any)[key] = [];
  } else {
    (this.searchCriteria as any)[key] = (key === 'statusId') ? 0 : null;
  }

      } else if (type === 'date') {
        (this.searchCriteria as any)[key] = null;
      }
      else if (type === 'toggle') {
        (this.searchCriteria as any)[key] = null;
      }

    });

    this.applyFilters();
  }

  toggleFilters() {
    this.filtersOpen = !this.filtersOpen;
  }

  // ---------- Sorting ----------

  sort(key: string) {
    if (this.sortColumn === key) {
      this.sortDirection = this.sortDirection === 'ASC' ? 'DESC' : 'ASC';
    } else {
      this.sortColumn = key;
      this.sortDirection = 'ASC';
    }

    (this.searchCriteria as any).sortColumn = this.sortColumn;
    (this.searchCriteria as any).sortDirection = this.sortDirection;

    this.applyFilters();
  }

  // ---------- Select All ----------

  toggleSelectAll(event: Event) {
    const checked = (event.target as HTMLInputElement).checked;

    this.data = this.data.map((item: any) => ({
      ...item,
      selected: checked
    }));
  }

  updateSelectedItems() {
    // لو حبيت تبعت selectedItems للـ parent بعدين
  }

  // ---------- Pagination ----------

  onPageChangeInternal(page: number) {
    this.page = page;
    this.pageChange.emit(page);
  }

  onEntriesChangeInternal() {
    this.entriesChange.emit(this.entries);
  }

  // ---------- Actions ----------

  editItem(id: number) {
    this.edit.emit(id);
  }

  deleteItem(id: number) {
    this.delete.emit(id);
  }

  // ---------- Export ----------

  exportExcel() {
    const table = document.querySelector('.generic-table-container table');
    if (!table) return;

    const html = (table as HTMLElement).outerHTML.replace(/ /g, '%20');
    const fileName = `export_${new Date().toISOString().slice(0, 10)}.xls`;
    const dataType = 'application/vnd.ms-excel';

    const link = document.createElement('a');
    link.href = 'data:' + dataType + ', ' + html;
    link.download = fileName;
    link.click();
  }

  // ---------- Column Resize ----------

  startResize(event: MouseEvent, key: string) {
    event.preventDefault();
    const th = (event.target as HTMLElement).closest('th') as HTMLElement;
    if (!th) return;

    this.resizingColumnKey = key;
    this.resizeStartX = event.pageX;
    this.resizeStartWidth = th.offsetWidth;

    document.addEventListener('mousemove', this.onMouseMove);
    document.addEventListener('mouseup', this.onMouseUp);
  }

  onMouseMove = (event: MouseEvent) => {
    if (!this.resizingColumnKey) return;
    const diff = event.pageX - this.resizeStartX;
    const newWidth = Math.max(this.resizeStartWidth + diff, 80);
    this.columnWidths[this.resizingColumnKey] = `${newWidth}px`;
  };

  onMouseUp = () => {
    this.resizingColumnKey = null;
    document.removeEventListener('mousemove', this.onMouseMove);
    document.removeEventListener('mouseup', this.onMouseUp);
  };

  getColumnStyle(key: string) {
    return this.columnWidths[key] ? { width: this.columnWidths[key] } : {};
  }

  ngOnDestroy(): void {
    document.removeEventListener('mousemove', this.onMouseMove);
    document.removeEventListener('mouseup', this.onMouseUp);
  }

@Input() rowClickableCondition?: (item: T) => boolean;

onRowClick(item: T, event: MouseEvent) {

  if (!this.rowClickable) return;

  const target = event.target as HTMLElement;
  if (
    target.closest('button') ||
    target.closest('input') ||
    target.closest('a')
  ) {
    return;
  }

  if (this.rowClickableCondition && !this.rowClickableCondition(item)) {
    return;
  }

  const id = this.getItemId(item);
  if (id !== undefined && id !== null) {
    this.edit.emit(id); 

  }
}



  onCheckboxChange(item: T, event: Event) {
    const checked = (event.target as HTMLInputElement).checked;

    this.checkboxChange.emit({
      row: item,
      checked
    });
  }

  isChecked(item: T): boolean {
    if (!this.checkboxKey) return false;

    const value = (item as any)[this.checkboxKey];
    return value === true;
  }
 exportPdf() {
    this.exportPdfClick.emit();
  }
  exportExcell() {
    this.exportExcelClick.emit();
  }
 
onToggleFilterClick(key: string, value: any) {
  (this.searchCriteria as any)[key] = value;
  (this.searchCriteria as any).pageIndex = 1;
  this.applyFilters();
}


}
