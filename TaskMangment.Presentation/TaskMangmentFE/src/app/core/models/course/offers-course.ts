export interface OfferAddEdit {
  title: string;
  description?: string;
  courseId?: number;
  subjectId?: number;
  startDate: string;
  endDate?: string;
}

export interface OfferGet {
  id: number;
  title: string;
  description?: string;
  courseTitle?: string;
  courseId?: number;
  subjectId?:number;
  subjectTitle?: string;
  assignedStudents: string[];
 assignedStudentsIds?: number[]; 
  startDate: string;
  endDate?: string;
}

export interface OfferAssignStudents {
  studentIds: number[];
}

export interface OfferPagedResponse {
  data:OfferGet[];
    totalCount: number;
    pageIndex: number;
    pageSize: number;
  
}

export interface OfferSentForm {
  offerTitle: string;
  offerDate: string;       // StartDate
  clientName: string;
  clientId: string;
  clientPhone: string;
  branch: string;
  paymentMethod: string;
  price: number;
  interestRate: number;
  discountRate: number;
  installmentValue: number;
  netAmount: number;
  offerBy: string;
  requiredSpecialization: string;
  notes: string;
}
