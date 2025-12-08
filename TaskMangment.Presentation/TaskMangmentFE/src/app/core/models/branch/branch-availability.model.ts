
export interface ModuleAvailability {
    id: number;
    name: string;
    activeFlag: boolean;
  }
  
  export interface BranchAvailabilityModel {
    branchCode: number;               
    modules: ModuleAvailability[];     
  }
  