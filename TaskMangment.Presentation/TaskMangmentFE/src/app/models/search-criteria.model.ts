export class SearchCriteria<T = any> {
  // Dynamic filter fields
  [key: string]: any;

  // Pagination & Sorting
  pageIndex: number = 1;
  pageSize: number = 10;
  sortColumn: string = 'Id';
  sortDirection: 'ASC' | 'DESC' = 'ASC';

  // Filter UI definition
  filterTypes?: { [key: string]: 'text' | 'dropdown' | 'date' | 'radio'| 'toggle' };

  constructor(init?: Partial<SearchCriteria<T>>) {
    Object.assign(this, init);
  }

  searchKey?: string;
  employeeIds?: number[];
  statusId?: number | null;
  direction?: number | null;
  targetEmployeeId?: number | null;
  priorityId?: number | null;
  createdFrom?: string | null;
  createdTo?: string | null;
  dueFrom?: string | null;
  dueTo?: string | null;
}