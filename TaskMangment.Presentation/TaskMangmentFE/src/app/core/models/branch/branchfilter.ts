export class Branchfilter {
    name: string ='';   
    pageNo: number = 1;
    pageCount: number = 10;
  
    constructor(init?: Partial<Branchfilter>) {
      Object.assign(this, init);
    }
}
