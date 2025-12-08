export class SearchCriteria<T = any> {
  [key: string]: any;
  pageNo: number = 1;
  pageCount: number = 10; 
  filterTypes?: { [key in keyof T]?: string }; // Make filterTypes generic

  constructor(init?: Partial<SearchCriteria<T>>) {
    Object.assign(this, init);
  }
}
