import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { TranslateModule } from '@ngx-translate/core';
import { SearchCriteria } from 'app/core/models/search-criteria.model';

export interface TableColumn {
  key: string;
  label: string;         // translation key أو نص عادي
  isButton?: boolean;    // لو عمود زرار
  icon?: string;         // للأعمدة اللي هي buttons
  onClick?: (item: any) => void
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
    NgbPaginationModule, TranslateModule
  ],
  styleUrls: ['./generic-table.component.scss']
})
export class GenericTableComponent<T> implements OnDestroy {

  @Input() formUrl: string = '';
  @Input() breadcrumbs: string[] = [];
  @Input() activeitem: string = ''; 

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

  @Input() searchCriteria!: SearchCriteria<T>;
  @Input() labels: { [key: string]: string } = {};
  @Input() statusOptions: { id: number; name: string }[] = [];

  // callback من الـ parent (زى Expiry)
  @Input() onSearch?: (criteria: SearchCriteria<T>) => void;
  @Input() onAddClick?: () => void;
  @Output() addClick = new EventEmitter<void>();

  // ---------- Outputs ----------
  @Output() pageChange = new EventEmitter<number>();
  @Output() entriesChange = new EventEmitter<number>();
  @Output() edit = new EventEmitter<number>();
  @Output() delete = new EventEmitter<number>();

  // ---------- UI State ----------
  loading: boolean = false;             // لو حبيت تستخدمه بعدين
  filtersOpen: boolean = true;

  sortColumn: string = '';
  sortDirection: 'ASC' | 'DESC' = 'ASC';

  // column resize state
  columnWidths: { [key: string]: string } = {};
  private resizingColumnKey: string | null = null;
  private resizeStartX: number = 0;
  private resizeStartWidth: number = 0;

  // ---------- Helpers ----------

  objectKeys(obj: any): string[] {
    return obj ? Object.keys(obj) : [];
  }

  getLabel(key: string): string {
    return this.labels?.[key] ?? key;
  }


  getInputType(key: string): 'text' | 'dropdown' | 'date' | 'number' {
    const filterTypes = (this.searchCriteria?.filterTypes || {}) as any;
    return filterTypes[key] || 'text';
  }

  getDropdownOptions(key: string) {
    if (key === 'statusId') return this.statusOptions;
    // تقدر تزود switch هنا لباقي الفلاتر
    return [];
  }

  // ---------- Filters ----------

  applyFilters() {
    if (this.onSearch) {
      // نمرر نسخة عشان منلعبش فى الريفرنس الأصلي
      this.onSearch({ ...(this.searchCriteria || {}) });
    }
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
        (this.searchCriteria as any)[key] = 0;
      } else if (type === 'date') {
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
}
